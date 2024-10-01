// CommandHandler.h

#ifndef _COMMANDHANDLER_h
#define _COMMANDHANDLER_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include <ArduinoJson.h>
#include <functional>
#include <queue>
#include "models.h"

class CommunicationHandler {
public:
	CommunicationHandler() = default;

	void update();
	void sendCommand(const CommandPacket& packet);
	void pushData(const CommandPacket& packet);
	CommandPacket getReceivedCommand() const;
	CommandPacket getSentCommand() const;

	using CommandCallBack = std::function<void(const CommandPacket&)>;
	void setCommandReceivedCallback(const CommandCallBack& callback);

private:
	void readSerial();
	void sendData();
	static CommandPacket deserializeBuffer(const String& json_string);
	void setReceivedCommand(const CommandPacket& command);

	CommandPacket command_last_sent_, command_last_received_;
	CommandCallBack command_received_callback_;
	String serial_data_ = "";

	std::queue<CommandPacket> data_queue_;
	unsigned long last_time_data_out_ = 0;
	const unsigned long data_out_rate_ = 200;
};

#endif