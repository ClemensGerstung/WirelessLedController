// #define USE_ADAFRUIT_NEOPIXEL
#define USE_ADAFRUIT_DOTSTAR

#include "Arduino.h"
#include "config.h"

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

#ifdef USE_ADAFRUIT_NEOPIXEL
Adafruit_NeoPixel strip = Adafruit_NeoPixel(LED_COUNT, LED_PIN, LED_MODE);
#endif
#ifdef USE_ADAFRUIT_DOTSTAR
Adafruit_DotStar strip = Adafruit_DotStar(LED_COUNT, LED_PIN, CLK_PIN, DOTSTAR_RGB);
#endif

void setup()
{
	strip.begin();

	WiFi.mode(WIFI_STA);
	WiFi.begin(SSID, PASSWORD);

	while (WiFi.status() != WL_CONNECTED)
	{
		delay(1000);
	}

	server.begin();
	strip.show();
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
						uint8_t data[2] =
						{ uint8_t(LED_COUNT >> 8), uint8_t(LED_COUNT) };
						client.write(data, 2);
					}
					else if (code == Codes::GET_LED)
					{
						uint16_t index;
						uint8_t data[4];

						// TODO: check read bytes
						client.read(data, 2);

						index = data[0] << 8 | data[1];

						uint32_t color = strip.getPixelColor(index);
						data[3] = color >> 24;
						data[0] = color >> 16;
						data[1] = color >> 8;
						data[2] = color;

						client.write(data, 4);
					}
					else if (code == Codes::GET_LEDS)
					{
						uint16_t from, to;
						uint8_t data[4];

						// TODO: check read bytes
						client.read(data, 4);

						from = data[0] << 8 | data[1];
						to = data[2] << 8 | data[3];

						// no boundaries check if on strip
						for (uint16_t index = from; index <= to; index++)
						{
							uint32_t color = strip.getPixelColor(index);
							data[3] = color >> 24;
							data[0] = color >> 16;
							data[1] = color >> 8;
							data[2] = color;

							client.write(data, 4);
						}
					}
					else if (code == Codes::GET_BRIGHTNESS)
					{
						uint8_t b = strip.getBrightness();
						client.write(&b, 1);
					}
					else if (code == Codes::SET_LED)
					{
						uint16_t index;
						uint8_t data[6];

						// TODO: check read bytes
						client.read(data, 6);

						index = data[0] << 8 | data[1];
#ifdef USE_ADASTAR_NEOPIXEL
						strip.setPixelColor(index, data[2], data[3], data[4], data[5]);
#endif
#ifdef USE_ADAFRUIT_DOTSTAR
						strip.setPixelColor(index, data[2], data[3], data[4]);
#endif
						strip.show();
					}
					else if (code == Codes::SET_LEDS)
					{
						uint16_t from, to;
						uint8_t data[4];

						// TODO: check read bytes
						client.read(data, 4);

						from = data[0] << 8 | data[1];
						to = data[2] << 8 | data[3];

						// no boundaries check if on strip
						for (uint16_t index = from; index <= to; index++)
						{
							client.read(data, 4);

#ifdef USE_ADASTAR_NEOPIXEL
							strip.setPixelColor(index, data[0], data[1], data[2], data[3]);
#endif
#ifdef USE_ADAFRUIT_DOTSTAR
							strip.setPixelColor(index, data[0], data[1], data[2]);
#endif
						}

						strip.show();
					}
					else if (code == Codes::SET_BRIGHTNESS)
					{
						uint8_t b;
						client.read(&b, 1);
						strip.setBrightness(b);
						strip.show();
					}
				}
			}

			delay(10);
		}

		client.stop();
	}
}
