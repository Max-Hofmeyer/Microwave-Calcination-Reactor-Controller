// TemperatureSensor.h

#ifndef _TEMPERATURESENSOR_h
#define _TEMPERATURESENSOR_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
	#include "WProgram.h"
#endif
#include <Adafruit_ADS1X15.h>
#include "wire.h"
#include <Adafruit_SPIDevice.h>
#include <Adafruit_I2CRegister.h>
#include <Adafruit_I2CDevice.h>
#include <Adafruit_BusIO_Register.h>

class TemperatureSensor {
public:
	TemperatureSensor() = default;

	void begin();
	double getTemperature();

private:
	Adafruit_ADS1115 adc_;
	uint8_t analog_out_channel_ = 0;
	static constexpr double adc_scale_ = 3.3 / 32767.0;
	static constexpr double voltage_scale_ = 5.0/3.09;
};

#endif
