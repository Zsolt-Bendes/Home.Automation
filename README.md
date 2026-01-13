# Garage door watcher
Project is about how I monitor my garage door when the door opens or closes.
## The Problem
- My garage door remote started to malfunction and the door opened and closed randomly.
- Additionally sometimes the kids leave the garage door open.
- Me and my wife would like to receive a e-mail when the door gets openned or closed.
- Send reminder e-mail every 10 minutes when the door is open.
## Given conditions
- A WiFi connection is available in the garage.
- The WiFi connection may experience intermittent instability and cannot be considered fully reliable.
### Power Availability
- A stable power source is present in the garage and can be used to supply the required hardware components.
### Messaging Infrastructure
- A message broker is available on the local network.
- The broker is RabbitMQ with the MQTT plugin installed, enabling MQTT-based communication between devices and backend services.
# Door status watcher
## Hardware and Tech stack
- **Hardware**: ESP32-DEVKIT-32UE-4M and MC-38W reed sensor  
- **Message type**: MQTT (message broker: RabbitMQ)  
- **Programming languages**: C/C++  
I'm using an ESP32 with an external antenna and a magnetic reed sensor as the input device. When the door status changes, the ESP32 sends an MQTT message indicating the new state. I'm using the [arduino-mqtt](https://github.com/256dpi/arduino-mqtt) library created by Joël Gähwiler. The message body is formatted in JSON.
``` C
 void sendToMQTT(bool doorStatus) {
  StaticJsonDocument<200> message;
  message["GarageId"] = GARAGE_ID;
    if(doorStatus)  {
    message["DoorStatus"] = 0; 
  }
  else {
    message["DoorStatus"] = 1; 
  }

  char messageBuffer[512];
  serializeJson(message, messageBuffer);

  mqtt.publish(PUBLISH_DOOR_STATE, messageBuffer);
}
```
The message contains a static GUID (in case I add more IoT devices the system) and the state it self. When status is `LOW` the door is has been openned.
## Dealing with Unreliable  WiFi and Volatile door state
The WiFi signal in the garage is is generally acceptable but, at times it becomes weak. For example, during rain or snow, the signal strength can drop significantly. To deal with this I added reconnect functionality.
``` C
void ConnectToWifi() {
  Serial.print("Connecting to Wifi");
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  WiFi.onEvent(WiFiEvent);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  WiFi.setSleep(false);

  Serial.println("Connected to wifi!");
  Serial.println();
}

void WiFiEvent(WiFiEvent_t event) {
  switch (event) {
    case ARDUINO_EVENT_WIFI_STA_DISCONNECTED:
      Serial.println("WiFi lost connection");
      WiFi.disconnect(true);
      ConnectToWifi();
      break;
    case ARDUINO_EVENT_WIFI_STA_GOT_IP:
      Serial.print("Connected. IP: ");
      Serial.println(network.localIP());
      break;
  }
}
```
The `ConnectToWifi` function adds an event handler (`WiFiEvent`) that triggers when the WiFi status changes. When a disconnection occurs, a reconnection is attemt is made. The connection to the message broker also needs to be re‑established. I solved this by adding the reconnection logic to the ESP32’s `loop` function.
``` C
void loop() {
  mqtt.loop();

   if (!mqtt.connected()) {
    connectToMQTT();
  }

//...
}
```
## Handling Transient Door States
The reed relay uses a magnet to detect whether the door is opening or closing. Because the door does not move perfectly smoothly, therefore the sensor can briefly fluctuate during movement, producing short‑lived or “phantom” state changes. These false triggers can cause multiple unwanted MQTT messages to be sent.  
To address this, I implemented a debounce mechanism. When the signal changes state, the system starts a 2‑second debounce timer. During this period, additional state changes are ignored. Only after the debounce interval has passed will the next event be processed. This ensures that only intentional, stable door state transitions generate MQTT messages.
``` C
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 2000;

void loop() {
//...
if ((millis() - lastDebounceTime) > debounceDelay) {
    if (reading != lastState) {
      lastDebounceTime = millis();

      if (lastState == LOW) {
        Serial.println("EVENT: Door OPENED");
        sendToMQTT(true);
        lastState = HIGH;
      } else {
        Serial.println("EVENT: Door CLOSED");
        sendToMQTT(false);
        lastState = LOW;
      }
    }
 }
}
```
# Message Consumer
To consume messages from RabbitMQ, I built an ASP.NET Core application that runs on my Raspberry Pi using Docker. Its primary responsibility is to process door‑status events published by the ESP32 device and apply them to the domain model using event sourcing.
## Tech Stack
- **Framework:** ASP.NET Core 10  
- **Message broker library:** [Wolverine](https://wolverine.netlify.app/)  
- **Database:** PostgreSQL  
- **ORM / Event Store:** [Marten](https://martendb.io/)  
- **Domain model:** DDD with Event Sourcing  
- **Email provider:** [Mailgun](https://login.mailgun.com/)  
- **Hosting:** Docker  
## Events
### Register Garage
This event exists for two main reasons:
- **Event Sourcing Requirement:**  
  Since the system uses event sourcing, an aggregate must be created before any domain events can be applied. The `RegisterGarage` event initializes the aggregate so that subsequent door‑status events can be processed correctly.
- **Design Choice and Learning Opportunity:**  
  I could have implemented this as an HTTP endpoint or a similar mechanism, but I wanted to experiment with a message‑driven approach and learn from it. Because this event is simply a consequence of choosing event sourcing, I intentionally kept this part lightweight and straightforward.
``` csharp
public sealed record RegisterGarage(Guid GarageId, GarageDoorStatus DoorStatus);

public static class RegisterGarageHandler
{
    public static async Task<bool> LoadAsync(
        RegisterGarage command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var garage = await session.LoadAsync<GarageView>(command.GarageId, cancellationToken);
        return garage is not null;
    }

    [Transactional]
    public static GarageRegistered Handle(
        RegisterGarage command,
        bool isRegistered,
        IDocumentSession session,
        TimeProvider timeProvider)
    {
        if (isRegistered)
        {
            return null!;
        }

        var evt = new GarageRegistered(command.GarageId, command.DoorStatus, timeProvider.GetLocalNow());
        session.Events.StartStream<Domain.Garages.Garage>(command.GarageId, evt);

        return evt;
    }
}
```
### Garage Door Status Changes
When a door‑status message is received, it informs the backend that the state of the garage door has changed. This event triggers the domain workflow responsible for updating the corresponding aggregate. The system then applies the new state, persists it through event sourcing, and makes it available for any downstream processes such as notifications, projections, or audit logs.
``` csharp
public sealed record UpdateGarageDoorStatus(Guid GarageId, GarageDoorStatus DoorStatus);

public static class UpdateGarageDoorStatusHandler
{
    public static Events Handle(
        UpdateGarageDoorStatus command,
        [WriteAggregate(nameof(UpdateGarageDoorStatus.GarageId))] Domain.Garages.Garage garage,
        TimeProvider timeProvider)
    {
        if (command.DoorStatus is GarageDoorStatus.Open)
        {
            return [new GarageDoorOpened(command.GarageId, timeProvider.GetLocalNow())];
        }

        return [new GarageDoorClosed(command.GarageId, timeProvider.GetLocalNow())];
    }
}
```
### Email Notification Side Effects
When the aggregate update succeeds, the system triggers an email notification as a side effect. This behavior is implemented within the event‑handling pipeline: once the new door state is applied and persisted, a follow‑up command is dispatched to the email‑sending component. This ensures that notifications are only sent after the domain state is successfully updated, maintaining consistency between the stored event stream and any external communication.
``` csharp
public static class GarageDoorOpenedHandler
{
    public static async Task Handle(
        GarageDoorOpened evt,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        await emailService.SendGarageDoorStateChangeMailAsync(GarageDoorStatus.Open, evt.HappenedAt, cancellationToken);
        await messageBus.SendAsync(new GarageDoorNotClosed(evt.GarageId).DelayedFor(10.Minutes()));
    }
}

public static class GarageDoorClosedHandler
{
    public static async Task Handle(
        GarageDoorClosed evt,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        await emailService.SendGarageDoorStateChangeMailAsync(GarageDoorStatus.Closed, evt.HappenedAt, cancellationToken);
    }
}

public static class GarageDoorNotClosedHandler
{
    public static async Task Handle(
        GarageDoorNotClosed evt,
        [ReadAggregate(nameof(GarageDoorNotClosed.GarageId))] Domain.Garages.Garage garage,
        IEmailService emailService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        if (garage.DoorStatus is GarageDoorStatus.Open)
        {
            await emailService.SendGarageDoorOpenReminderMailAsync(cancellationToken);
            await messageBus.SendAsync(evt.DelayedFor(10.Minutes()));
        }
    }
}
```
# Possible improvements
- Add mDNS support to the ESP32.
- Add additional devices, such as a camera feed or remote door‑control functionality on the local network.
# Things that I learned
- MQTT topics use `/` as the separator.
- RabbitMQ maps MQTT topics to `.` (e.g., `garagedoor/door` → `garagedoor.door`).
- Wolverine interoperability: without using Wolverine’s envelope, only one message type can be parsed.
- The ESP32 does not resolve DNS names by default.
