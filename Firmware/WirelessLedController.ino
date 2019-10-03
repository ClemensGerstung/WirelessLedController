#include "Arduino.h"

#include <Adafruit_NeoPixel.h>
#include <ESP8266WiFi.h>

constexpr char* ssid = "<rename>";
constexpr char* password = "<set>";

WiFiServer server(80);

void setup()
{
	Serial.begin(115200);

	WiFi.begin(ssid, password);

	while (WiFi.status() != WL_CONNECTED) {
		delay(1000);
		Serial.println("Connecting...");
	}

	Serial.print("Connected to WiFi. IP: ");
	Serial.println(WiFi.localIP());

	server.begin();
}


void loop()
{

}
