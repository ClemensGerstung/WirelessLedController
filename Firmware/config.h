#ifndef CONFIG_H
#define CONFIG_H

#include <Adafruit_NeoPixel.h>

constexpr const char* SSID = "<rename>";
constexpr const char* PASSWORD = "<set>";
constexpr uint16_t PORT = 1337;

constexpr uint8_t LED_PIN = 0;
constexpr uint16_t LED_COUNT = 1 /* tbd. */;

#if defined(ADAFRUIT_NEOPIXEL_H)
constexpr neoPixelType LED_MODE = NEO_GRBW + NEO_KHZ800;
#endif

#endif
