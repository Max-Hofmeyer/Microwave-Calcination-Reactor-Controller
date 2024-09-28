#ifndef _CONTROLLOOP_h
#define _CONTROLLOOP_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif
#include "models.h"

class ControlLoop {
public:
	/*ControlLoop(double target_temp_val, double hysteresis_val, double delta_temp_val, double hold_time_val);*/
	ControlLoop() = default;
	void configure(const TestSpecPacket& test_specs);
	
	bool update(double current_temp_val);
	void reset();
	unsigned long getTotalSetpointTime();
private:

	TestSpecPacket test_specs_;
	unsigned long total_setpoint_time_;
	bool relay_on_, set_point_reached_ = false;
};

#endif

