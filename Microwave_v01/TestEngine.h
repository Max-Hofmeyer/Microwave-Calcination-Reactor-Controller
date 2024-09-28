// TestEngine.h

#ifndef _TESTENGINE_h
#define _TESTENGINE_h
#define RELAY_PIN 32
#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif
#include <functional>
#include "ControlLoop.h"
#include "TestState.h"
#include "CommunicationHandler.h"
#include "TemperatureSensor.h"

class TestEngine {
public:
	TestEngine();

	void update();
	void onCommandReceived(const CommandPacket& command_packet);

	using CommandCallBack = std::function<void(const CommandPacket&)>;
	using DataCommandCallBack = std::function<void(const CommandPacket&)>;
	void setCommandCallback(const CommandCallBack& callback);
	void setDataCommandCallback(const DataCommandCallBack& callback);

private:
	void onIdle();
	void onStart();
	void onStop();
	void onUnknown();
	void sendData(const DataPacket& data_packet);
	void pingBackCommand();
	void callbackDataCommand(const CommandPacket& command_packet);

	TestState test_state_;
	TemperatureSensor temperature_sensor_;
	ControlLoop control_loop_;

	CommandPacket last_received_command_;
	TestSpecPacket test_specs_;
	CommandCallBack send_command_callback_;
	DataCommandCallBack send_data_command_callback_;

	unsigned long last_time_data_out_ = 0;
	const unsigned long data_collection_rate_ = 200;
	//unsigned long test_duration_;
	//const unsigned long data_interval_ = 150; //ms

};

#endif

