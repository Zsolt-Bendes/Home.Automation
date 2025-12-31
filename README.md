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
# Solution
## Door status watcher
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
### Dealing with unrealible WiFi and volatial door state
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

