#include <WiFi.h>
#include <MQTTClient.h>
#include <ArduinoJson.h>

const char* WIFI_SSID = "";
const char* WIFI_PASSWORD = "";

const char* GARAGE_ID = "59652B18-3AFB-415D-80FE-AE92532203DD";
const char* MQTT_BROKER_ADRRESS = "";
const int MQTT_PORT = 1883;
const char* MQTT_CLIENT_ID = "Garage-esp-01";
const char* MQTT_USERNAME = "";
const char* MQTT_PASSWORD = "";

const char* PUBLISH_DOOR_STATE = "garagedoor/door";
const char* PUBLISH_REGISTER_DEVICE = "garageregister/register";

const int DOOR_PIN = 19;

bool registered;
int lastState = HIGH;

unsigned long lastReconnectAttempt = 0;
WiFiClient network;
MQTTClient mqtt = MQTTClient(256);

unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 2000;

void setup() {
  Serial.begin(115200);
  pinMode(DOOR_PIN, INPUT_PULLUP); 

  WiFi.mode(WIFI_STA);

  ConnectToWifi();

  connectToMQTT();
}

void loop() {
  mqtt.loop();

   if (!mqtt.connected()) {
    connectToMQTT();
  }

  int reading = digitalRead(DOOR_PIN);
  if(!registered){
    lastState = reading;
    sendToMQTTRegister(reading);
    registered = true;
  }

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

void connectToMQTT() {
  // Connect to the MQTT broker
  mqtt.begin(MQTT_BROKER_ADRRESS, MQTT_PORT, network);

  Serial.print("ESP32 - Connecting to MQTT broker");

  while (!mqtt.connect(MQTT_CLIENT_ID, MQTT_USERNAME, MQTT_PASSWORD)) {
    Serial.print(".");
    delay(100);
  }
  Serial.println();

  if (!mqtt.connected()) {
    Serial.println("ESP32 - MQTT broker Timeout!");
    return;
  }

  Serial.println("ESP32 - MQTT broker Connected!");
}

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

  Serial.println("ESP32 - sent to MQTT:");
  Serial.print("- topic: ");
  Serial.println(PUBLISH_DOOR_STATE);
  Serial.print("- payload:");
  Serial.println(messageBuffer);
}

void sendToMQTTRegister(bool doorStatus) {
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

  mqtt.publish(PUBLISH_REGISTER_DEVICE, messageBuffer);

  Serial.println("ESP32 - sent to MQTT:");
  Serial.print("- topic: ");
  Serial.println(PUBLISH_REGISTER_DEVICE);
  Serial.print("- payload:");
  Serial.println(messageBuffer);
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
