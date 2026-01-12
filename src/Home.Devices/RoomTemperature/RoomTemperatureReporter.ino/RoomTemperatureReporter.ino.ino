#include <WiFi.h>
#include <MQTTClient.h>
#include <ArduinoJson.h>
#include <AM2302-Sensor.h>

#define DHT22_PIN  15

const char* WIFI_SSID = "";
const char* WIFI_PASSWORD = "";

const char* ROOM_ID = "";

const int MQTT_PORT = 1883;
const char* MQTT_BROKER_ADRRESS = "";
const char* MQTT_CLIENT_ID = "";
const char* MQTT_USERNAME = "";
const char* MQTT_PASSWORD = "";

const char* PUBLISH_ROOM_Temp = "rooms/temp";

//every 10 minutes
const int REPORT_INTERVAL = 600000;

WiFiClient network;
MQTTClient mqtt = MQTTClient(256);
AM2302::AM2302_Sensor am2302(DHT22_PIN);

void setup() {
  Serial.begin(115200);

  WiFi.mode(WIFI_STA);

  connectToWifi();
  connectToMQTT();

  am2302.begin();
}

void loop() {
  mqtt.loop();

   if (!mqtt.connected()) {
    connectToMQTT();
  }

  auto status = am2302.read();

  if(status == 0)
  {
    sendToMQTT(am2302.get_Temperature(), am2302.get_Humidity());
  }

  delay(REPORT_INTERVAL);
}

void connectToWifi() {
  Serial.print("Connecting to Wifi");
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  WiFi.onEvent(wiFiEvent);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  WiFi.setSleep(false);

  Serial.println("Connected to wifi!");
  Serial.println();
}

void wiFiEvent(WiFiEvent_t event) {
  switch (event) {
    case ARDUINO_EVENT_WIFI_STA_DISCONNECTED:
      Serial.println("WiFi lost connection");
      WiFi.disconnect(true);
      connectToWifi();
      break;
    case ARDUINO_EVENT_WIFI_STA_GOT_IP:
      Serial.print("Connected. IP: ");
      Serial.println(network.localIP());
      break;
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

void sendToMQTT(float tempInC, float humidity) {
  StaticJsonDocument<200> message;
  message["RoomId"] = ROOM_ID;
  message["Temperature"] = tempInC;
  message["Humidity"] = humidity;

  char messageBuffer[512];
  serializeJson(message, messageBuffer);

  mqtt.publish(PUBLISH_ROOM_Temp, messageBuffer);

  Serial.println("ESP32 - sent to MQTT:");
  Serial.print("- topic: ");
  Serial.println(PUBLISH_ROOM_Temp);
  Serial.print("- payload:");
  Serial.println(messageBuffer);
}