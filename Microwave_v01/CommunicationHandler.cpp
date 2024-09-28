#include "CommunicationHandler.h"

//checks if there is info to be read in, or data to be sent out
void CommunicationHandler::update() {
	readSerial();
	sendData();
}

//send command packet through serial port 
void CommunicationHandler::sendCommand(const CommandPacket &packet, bool is_data) {
	JsonDocument serialize_command;
	serialize_command["Command"] = packet.command;
	serialize_command["Checksum"] = packet.checksum;

	const auto test_specs = serialize_command["TestSpecs"].to<JsonObject>();
	test_specs["TargetTemp"] = packet.test_spec_packet.target_temp;
	test_specs["DeltaTemp"] = packet.test_spec_packet.delta_temp;
	test_specs["TargetHoldTime"] = packet.test_spec_packet.target_hold_time;

	const auto data_packet = serialize_command["DataPacket"].to<JsonObject>();
	data_packet["TemperatureValue"] = packet.data_packet.temperature_value;
	data_packet["WallPowerValue"] = packet.data_packet.wall_power_value;
	data_packet["MagnetronPowerValue"] = packet.data_packet.magnetron_power_value;
	data_packet["ReflectedPowerValue"] = packet.data_packet.reflected_power_value;

	String json_stream;
	serializeJson(serialize_command, json_stream);
	json_stream += ":d:";

	command_last_sent_ = packet;
	Serial.print(json_stream);
	//Serial2.print(json_stream);
}

//pushes data onto the internal data queue
void CommunicationHandler::pushData(const CommandPacket& packet) {
	data_queue_.push(packet);
}

//returns the last command received from the application
CommandPacket CommunicationHandler::getReceivedCommand() const {
	return command_last_received_;
}

//returns the last state based command (ie not data commands)
CommandPacket CommunicationHandler::getSentCommand() const {
	return command_last_sent_;
}

//set what function to invoke when a command is deserialized
void CommunicationHandler::setCommandReceivedCallback(const CommandCallBack& callback) {
	command_received_callback_ = callback;
}

void CommunicationHandler::readSerial() {
	while (Serial.available()) {
		char char_on_buffer = Serial.read();
		serial_data_ += char_on_buffer;

		if (!serial_data_.endsWith(":d:")) return;
		serial_data_.trim();

		const auto command = deserializeBuffer(serial_data_);
		setReceivedCommand(command);

		serial_data_.clear();
	}
}

void CommunicationHandler::sendData() {
	if (data_queue_.empty()) return;

	const unsigned long current_time = millis();

	if (current_time - last_time_data_out_ >= data_out_rate_) {
		const CommandPacket data = data_queue_.front();
		sendCommand(data, true);

		last_time_data_out_ = current_time;
		data_queue_.pop();
	}

}

CommandPacket CommunicationHandler::deserializeBuffer(const String& json_string) {
	CommandPacket packet;
	JsonDocument deserialize_command;
	DeserializationError error = deserializeJson(deserialize_command, json_string);

	//todo add logging
	if (error) {
		//Serial2.println("Error while deserializing");
		return packet;
	}

	packet.command = deserialize_command["Command"];
	packet.checksum = deserialize_command["Checksum"];
	if (!deserialize_command["TestSpecs"].isNull()) {
		JsonObject test_specs = deserialize_command["TestSpecs"];
		packet.test_spec_packet.target_temp = test_specs["TargetTemp"];
		packet.test_spec_packet.delta_temp = test_specs["DeltaTemp"];
		packet.test_spec_packet.target_hold_time = test_specs["TargetHoldTime"];
	}

	if (!deserialize_command["DataPacket"].isNull()) {
		const JsonObject data_packet = deserialize_command["DataPacket"];
		packet.data_packet.temperature_value = data_packet["TemperatureValue"];
		packet.data_packet.wall_power_value = data_packet["WallPowerValue"];
		packet.data_packet.magnetron_power_value = data_packet["MagnetronPowerValue"];
		packet.data_packet.reflected_power_value = data_packet["ReflectedPowerValue"];
	}

	return packet;
}

void CommunicationHandler::setReceivedCommand(const CommandPacket& command) {
	command_last_received_ = command;
	if (command_received_callback_) {
		command_received_callback_(command);
	}
}

