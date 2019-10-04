#include "Arduino.h"

#include "config.h"

#include <Adafruit_NeoPixel.h>
#include <ESP8266WiFi.h>

enum Codes : uint8_t
{
	COUNT = 0x01,
	SET_LED = 0x02,
	GET_LED = 0x03,
	SET_LEDS = 0x04,
	GET_LEDS = 0x05,
	SET_BRIGHTNESS = 0x06,
	GET_BRIGHTNESS = 0x07
};

WiFiServer server(PORT);

#if defined(ADAFRUIT_NEOPIXEL_H)
Adafruit_NeoPixel strip = Adafruit_NeoPixel(LED_COUNT, LED_PIN, LED_MODE);
#endif

void setup()
{
	Serial.begin(115200);

	WiFi.begin(SSID, PASSWORD);

	while (WiFi.status() != WL_CONNECTED)
	{
		delay(1000);
		Serial.println("Connecting...");
	}

	Serial.print("Connected to WiFi. IP: ");
	Serial.println(WiFi.localIP());

	server.begin();

#if defined(ADAFRUIT_NEOPIXEL_H)
	strip.begin();
#endif
}

void loop()
{
	WiFiClient client = server.available();

	if (client)
	{
		while (client.connected())
		{
			while (client.available() > 0)
			{
				uint8_t code;
				int read = client.read(&code, 1);

				if (read == 1)
				{
					if (code == Codes::COUNT)
					{
						// send count of LEDs
						// TODO: maybe check written bytes == 2
						uint8_t data[2] = { uint8_t(LED_COUNT >> 8), uint8_t(LED_COUNT) };
						client.write(data, 2);
					}
					else if (code == Codes::GET_LED)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint16_t index;
						uint8_t data[4];

						// TODO: check read bytes
						client.read(data, 2);

						index = data[0] << 8 | data[1];
						Serial.println(index);

						uint32_t color = strip.getPixelColor(index);
						data[0] = color >> 24;
						data[1] = color >> 16;
						data[2] = color >> 8;
						data[3] = color;

						client.write(data, 4);
#endif
					}
					else if(code == Codes::GET_LEDS)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint16_t from, to;
						uint8_t data[6];

						// TODO: check read bytes
						client.read(data, 4);

						from = data[0] << 8 | data[1];
						to = data[2] << 8 | data[3];
						Serial.println(from);
						Serial.println(to);

						// no boundaries check if on strip
						for(uint16_t index = from; index <= to; index++)
						{
							data[0] = index >> 8;
							data[1] = index;

							uint32_t color = strip.getPixelColor(index);
							data[2] = color >> 24;
							data[3] = color >> 16;
							data[4] = color >> 8;
							data[5] = color;

							client.write(data, 6);
						}
#endif
					}
					else if(code == Codes::GET_BRIGHTNESS)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint8_t b = strip.getBrightness();
						client.write(&b, 1);
#endif
					}
					else if(code == Codes::SET_LED)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint16_t index;
						uint8_t data[6];

						// TODO: check read bytes
						client.read(data, 6);

						index = data[0] << 8 | data[1];

						strip.setPixelColor(index, data[2], data[3], data[4], data[5]);
						strip.show();
#endif
					}
					else if(code == Codes::SET_LEDS)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint16_t from, to;
						uint8_t data[4];

						// TODO: check read bytes
						client.read(data, 4);

						from = data[0] << 8 | data[1];
						to = data[2] << 8 | data[3];

						// no boundaries check if on strip
						for(uint16_t index = from; index <= to; index++)
						{
							client.read(data, 4);

							strip.setPixelColor(index, data[0], data[1], data[2], data[3]);
						}

						strip.show();
#endif
					}
					else if(code == Codes::SET_BRIGHTNESS)
					{
#if defined(ADAFRUIT_NEOPIXEL_H)
						uint8_t b;
						client.read(&b, 1);
						strip.setBrightness(b);
						strip.show();
#endif
					}
				}
			}

			delay(10);
		}

		client.stop();
		Serial.println("Client disconnected");
	}
}
