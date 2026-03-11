# Humidity and temperature sensor
This part of the project was focusing on adding the ability to add some in house monitoring capabilites. In more details I wanted to know if we need to open the windows and let some fresh air in.
## The Problem
- High humidity can lead to mold
- We have multiple rooms that will need monitoring
- Track daily statistics like min, max, average of temperature and humidity
- I don't want to go the db to check the values. I  will need some sort of a dashboard 
## Given conditions
- I had a couple of spare ESP32-C6 around
- I had a couple of spare of DHT22 sensors
- No energy constrains
- A message broker is available on the local network.
- The broker is RabbitMQ with the MQTT plugin installed, enabling MQTT-based communication between devices and backend services.
# Temperature and Humidity sensor
## DHT22
This is a well known ceramic temperature and humidity sensor. The arduino community has a couple of libraries available.
I have chosen [AM2302-Sensor](https://github.com/hasenradball/AM2302-Sensor) library. It is easy to use and it works.
### Reading values 
``` C
#include <AM2302-Sensor.h>

#define DHT22_PIN 1
AM2302::AM2302_Sensor am2302(DHT22_PIN);

void setup() {
    am2302.begin();

    auto status = am2302.read();
    do {
        delay(210);
        status = am2302.read();
    } while (status != 0);

  sendToMQTT(am2302.get_Temperature(), am2302.get_Humidity());
}
```

Just a couple of lines of code to get the values from the sensor. I used a loop because sometimes reading from the sensor can fail. (I know the loop could be improved using a regular while to avoid a second sensor read if the first one succeeds. Room for improvement.)
## Sending measurements to RabbitMQ
Sending values to the bus is similar how the door sensor is working.(See part 1.) The temp sensor has a dedicated queue `roomtemp` that is mapped to the `room/temp` MQTT messages.
A measurement is taken every 5 minutes and send to the queue.
``` json
{ 
    "SensorId": "019c2e07-c1e5-778e-86b2-7cca2cebcb51", 
    "Humidity": 48,
    "Temperature": 20 
}
```
The humidity is between 0 and 100. Temperature value is read and send as Celsius value.

## Deep sleep
To learn something new I wanted to look into and try out ESP32 deep sleep cababilites. Turns out it is simple to use.
By including the `esp_sleep.h` header file we get access to API.
When using deep sleep you have a couple of ways to wake the device up. I used the timer approach as I want to do a measurement every 5 minutes. To enable this I used the following 2 instructions.
``` C
esp_sleep_enable_timer_wakeup(SLEEP_TIME);
esp_deep_sleep_start();
```
There are some traps that needs to be aveare of:
- When waking up insert a small delay to make sure the system boots 
- The loop method is going to be empty (in my case)
``` C
void setup() {
  delay(100);

  // perform measurement and message sending
  // ...

  esp_sleep_enable_timer_wakeup(SLEEP_TIME);
  esp_deep_sleep_start();
}

void loop() {
}
```
# Consuming the message
## Message handler
The message consumer is quite straith forward. Like a regular message handler in Wolverine.
``` csharp
public sealed record TemperatureMeasurement(Guid SensorId, double Temperature, double Humidity);

public static class TemperatureMeasurementHandler
{
    public static TemperatureMeasurementReceived Handle(
        TemperatureMeasurement command,
        [WriteAggregate(nameof(TemperatureMeasurement.SensorId))] TemperatureAndHumiditySensor sensor,
        TimeProvider timeProvider)
    {
        return new TemperatureMeasurementReceived(
            command.SensorId,
            command.Temperature,
            command.Humidity,
            timeProvider.GetLocalNow());
    }

    [WolverineAfter]
    public static async Task SendLiveUpdate(
        TemperatureMeasurement command,
        [ReadAggregate(nameof(TemperatureMeasurement.SensorId))] TemperatureAndHumiditySensor sensor,
        IHubContext<LiveUpdater> liveUpdater,
        CancellationToken cancellationToken)
    {
        await liveUpdater.Clients.All.SendAsync(
            "room_temp_update",
            new LiveTempData(
                command.SensorId,
                command.Temperature,
                command.Humidity,
                sensor.MaxTemperature,
                sensor.MinTemperature,
                sensor.AverageTemperature,
                sensor.MaxHumidity,
                sensor.MinHumidity,
                sensor.AverageHumidity),
            cancellationToken);
    }
}
```
As you can see I'm using SignalR Core to notify the frontend about live updates. When ever the values are calculated a event is send to the frontend.

## Calculating daily statistics
As I'm using event sourcing I need to apply the calculations on the write side (the aggregate) and on view (the dashboard). For avoiding business logic code I created a `static class` that does the calculation.
``` csharp
public static class StatisticsCalculator
{
    public static TemperatureMeasurementStatisticsUpdated CalculateStatistics(
        DateTimeOffset? prevMeasurementDate,
        int prevMeasurementCount,
        double minTemperature,
        double maxTemperature,
        double sumOfTemperature,
        double minHumidity,
        double maxHumidity,
        double sumOfHumidity,
        TemperatureMeasurementReceived newMeasurement)
    {
        if (prevMeasurementDate is null)
        {
            minTemperature = newMeasurement.TemperatureInCelsius;
            minHumidity = newMeasurement.Humidity;
            maxTemperature = newMeasurement.TemperatureInCelsius;
            maxHumidity = newMeasurement.Humidity;
        }

        if (prevMeasurementDate?.Day == newMeasurement.MeasuredAt.Day)
        {
            prevMeasurementCount++;

            if (newMeasurement.Humidity > 0)
            {
                sumOfHumidity += newMeasurement.Humidity;
            }

            if (newMeasurement.TemperatureInCelsius > 0)
            {
                sumOfTemperature += newMeasurement.TemperatureInCelsius;
            }

            return new TemperatureMeasurementStatisticsUpdated(
                newMeasurement.SensorId,
                prevMeasurementCount,
                sumOfTemperature / prevMeasurementCount,
                sumOfHumidity / prevMeasurementCount,
                minTemperature > newMeasurement.TemperatureInCelsius ? newMeasurement.TemperatureInCelsius : minTemperature,
                minHumidity > newMeasurement.Humidity ? newMeasurement.Humidity : minHumidity,
                maxTemperature < newMeasurement.TemperatureInCelsius ? newMeasurement.TemperatureInCelsius : maxTemperature,
                maxHumidity < newMeasurement.Humidity ? newMeasurement.Humidity : maxHumidity,
                sumOfTemperature,
                sumOfHumidity);
        }

        return new TemperatureMeasurementStatisticsUpdated(
            newMeasurement.SensorId,
            1,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity);
    }
}
```
For each measurment we do a check if the incoming measurment is on the same day as the prev. ones. If yes, then I increment measurements counter and then add the new values to the sums. We need this for the average calculations. The second part of the scope is checking for min and max values.
In case it is new day measurement then we basically reset the sums and counter.

# The dashboard

# Possible improvements
- OTA (over the air) update for the sensors
- Notification feature when the humidity goes above 60%

# Known issues
- Temp sensor fails to restart after a while (investigating)
- Minimum temperature and humidity values are not correclty updated (investigating)

# Things that I learned
- Nginx config for using websocket
- bash scripts for speeding up deployments (Those are not included in the repository as well as the docker compose file!)
- ESP32 sleep modes and wake up triggers
- Tried out `TUnit` and I find it easy to use, fast (I only have a couple of tests ATM) with a lot of good features.