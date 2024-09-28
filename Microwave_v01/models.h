#pragma once

typedef enum {
	INIT,
	START,
	STOP,
	DATA,
	BAD_COMMAND,
	DEBUG,
}commands_t;

enum class State {
	IDLE,
	RUNNING,
	STOPPED,
	UNKNOWN
};

struct TestSpecPacket {
	double target_temp, delta_temp, target_hold_time = 0.0;
};

struct DataPacket {
	double temperature_value, wall_power_value, magnetron_power_value, reflected_power_value = 0.0;
};

struct CommandPacket {
	commands_t command;
	uint8_t checksum;
	DataPacket data_packet;
	TestSpecPacket test_spec_packet;
};
