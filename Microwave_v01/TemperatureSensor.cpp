#include "TemperatureSensor.h"

void TemperatureSensor::begin() {
	if(adc_.begin())
	adc_.setGain(GAIN_ONE);
}

double TemperatureSensor::getTemperature() {

	int16_t adc_val = adc_.readADC_SingleEnded(analog_out_channel_);
	if (adc_val == 0x8000) {
		//Serial2.println("ADC overflowed");
	}
	const double sensor_voltage = (adc_val * adc_scale_) * voltage_scale_;
	const double temperature = (sensor_voltage * 216.0);

	return temperature;
}