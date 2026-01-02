# Garage door watcher
Project is about how I log when someone opens or closes my garage door.
## The Problem
- My garage door remotes started to mailfunction and the door open and close the door randomly.
- Additionally sometimes the kids leave the garage door open.
- Me and my wife would like to receive a e-mail when the door gets openned or closed.
## Given conditions
- WiFi connection available in the garage
- The WiFi connection is not 100% realible
- Power source is available in the garage
- Message broker exists on the local net
    - RabbitMq with MQTT pluging installed
# Door status watcher
## Hardware and Tech stack
-  Hardware: ES32-DEVKIT-32UE-4M and Reed sendsor MC-38W
-  Message type: MQTT (message broker is RabbitMq)
-  Programing languages: C/C++
I'm using a ESP32 with a antenna and a magnetic reed relay as a sensor. When the door status changes then the ESP32 sends a MQTT message that the door status changed. I'm using [arduino-mqtt](https://github.com/256dpi/arduino-mqtt) made by Joël Gähwiler. The body of the message is in JSON format.
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
The message contains a static GUID (just in case I add more IoT devices the system) and the state it self. When status is `LOW` the door is has been openned.
## Dealing with unrealible WiFi and volatial door state
The WiFi signal in the garage is ok but some times the signal is weak or none existing. (Example when it is raining or snowing the signal is far weaker.) To deal with this I added reconnect functionality.
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
The `ConnectToWifi` add event handler (`WiFiEvent`) when the WiFi status changes. When disconnected then reconnection is attemted. Additionally in this case the connection to the message broker needs to be restored as well. I solved this by adding the reconnect to `loop` function of the ESP.
``` C
void loop() {
  mqtt.loop();

   if (!mqtt.connected()) {
    connectToMQTT();
  }

//...
}
```
Now for handling the transient state when the door opens or closes.  
The reed relay uses a magnet to detect if the door is closing or opening. The closing and opening process is not so smoth so "phantom" events can happen. To prevent this I build in a debounce timer when the signal state changes there is a 2 seconds wait time until the next event can be processed.
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
# Messag consumer
For consuming the messages from RabbitMq I build a ASP.NET core application that is hosted on my raspberry pi with docker.
## Tech stack
-   ASP.NET core 10
-   Message broker library: [Wolverine](https://wolverine.netlify.app/)
-   Database: Postgres
-   ORM: [Marten](https://martendb.io/)
-   Domain is in DDD style with Even Sourcing
-   Mail sender provider: [Mailgun](https://login.mailgun.com/)
-   Hosting: docker
## Events
### Register garage
There are two reasons for the existence of this message  
- I'm using event sourcing. This means I need to create a aggregate first before I can forward any events.
    - I could have choosen a http endpoint for this or something similiar but wanted to try out something new and learn from this.
- I did not wanted to put much time and effort into this part as this is just a consequence of my decision of choosing event sourcing
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
### Garage door status changes
The message essentially tells the backend that the door status has been changed. This invokes the flow of updating the aggregate.
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
 When the update succeeds then the e-mail sending is triggered as a side effect(s).
``` csharp
public static class GarageDoorOpenedHandler
{
    public static async Task Handle(
        GarageDoorOpened evt,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        await emailService.SendGarageDoorStateChangeMailAsync(GarageDoorStatus.Open, evt.HappenedAt, cancellationToken);
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
```
# Possible improvements
- Adding mDNS to the ESP32
- Add additional devices like camera feed, the ability to open/close the door from local networt
# Things that I learned
- MQTT queue naming is with `/`
- RabbitMq will map the MQTT queue to `.`. Example `garagedoor/door` will be mapped to `garagedoor.door`
- Interoperability in Wolverine: when not using the envelope of Wolverine only 1 message type can be parsed
- By default ESP32 doest not resolve DNS names
