#pragma once
#include <string>

typedef enum {
	INIT,
	START,
	STOP,
	COOLDOWN,
	DATA,
	BAD_COMMAND,
	DEBUG,
}commands_t;

enum class State {
	IDLE,
	RUNNING,
	COOLDOWN,
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
	std::string debug_message = "";

	//default
	CommandPacket()
		: command(BAD_COMMAND), checksum(0), data_packet(), test_spec_packet() {}

	//DataPacket
	CommandPacket(commands_t cmd, uint8_t chksum, const DataPacket& data)
		: command(cmd), checksum(chksum), data_packet(data), test_spec_packet() {}

	//TestSpecPacket
	CommandPacket(commands_t cmd, uint8_t chksum, const TestSpecPacket& test_spec)
		: command(cmd), checksum(chksum), data_packet(), test_spec_packet(test_spec) {}

	//DebugMessage
	CommandPacket(commands_t cmd, uint8_t chksum, const std::string& debug_msg)
		: command(cmd), checksum(chksum), data_packet(), test_spec_packet(), debug_message(debug_msg) {}

	//basic
	CommandPacket(commands_t cmd, uint8_t chksum)
		: command(cmd), checksum(chksum), data_packet(), test_spec_packet() {}

};
