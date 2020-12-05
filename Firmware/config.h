#ifndef CONFIG_H
#define CONFIG_H

#ifdef USE_ADAFRUIT_NEOPIXEL
#include <Adafruit_NeoPixel.h>
#endif

#ifdef USE_ADAFRUIT_DOTSTAR
#include <Adafruit_DotStar.h>
#endif

constexpr const char* SSID = "<ssid>";
constexpr const char* PASSWORD = "<pin>";
constexpr uint16_t PORT = 1337;

constexpr uint8_t LED_PIN = 2;
constexpr uint8_t CLK_PIN = 0;
constexpr uint16_t LED_COUNT = 60;

#ifdef USE_ADAFRUIT_NEOPIXEL
constexpr neoPixelType LED_MODE = NEO_GRBW + NEO_KHZ800;
#endif

#endif
