#include "ControlLoop.h"

void ControlLoop::configure(const TestSpecPacket& test_specs) {
	test_specs_ = test_specs;
	test_specs_.target_hold_time *= 1000; //convert seconds to milliseconds
	total_setpoint_time_ = 0;
}

bool ControlLoop::update(double current_temp_val) {
	unsigned long current_time = millis();
	if (current_temp_val < test_specs_.target_temp - test_specs_.delta_temp) {
		/*Serial2.println("relay on");*/
		relay_on_ = true;
		total_setpoint_time_ = 0;
	}
	else if (current_temp_val >= test_specs_.target_temp + test_specs_.delta_temp) {
		/*Serial2.println("relay off");*/
		relay_on_ = false;
		if (total_setpoint_time_ == 0) {
			total_setpoint_time_ = current_time;
		}
	}
	//todo check to make sure this works 
	else {
		/*Serial2.println("relay maintaining");*/
		total_setpoint_time_ += current_time;
	}
	return relay_on_;
}

void ControlLoop::reset() {
	/*Serial2.println("control loop resetting");*/
	relay_on_ = false;
	total_setpoint_time_ = 0;

}

unsigned long ControlLoop::getTotalSetpointTime() {
	return total_setpoint_time_;
}

