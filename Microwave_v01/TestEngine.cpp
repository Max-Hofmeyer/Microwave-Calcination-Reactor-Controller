#include "TestEngine.h"

TestEngine::TestEngine() : test_specs_() {
	test_state_.setInitCallback(std::bind(&TestEngine::onIdle, this));
	test_state_.setRunningCallback(std::bind(&TestEngine::onStart, this));
	test_state_.setShutdownCallback(std::bind(&TestEngine::onStop, this));
	test_state_.setUnknownCallback(std::bind(&TestEngine::onUnknown, this));
}

//if a test is running, grab temperature data, update the control loop, and raise the data ready event
void TestEngine::update() {
	if (test_state_.getCurrentState() != State::RUNNING) return;

	unsigned long current_time = millis();

	if (current_time - last_time_data_out_ >= data_collection_rate_) {
		double temperature = temperature_sensor_.getTemperature();
		if (control_loop_.update(temperature)) {
			digitalWrite(RELAY_PIN, HIGH);
		}
		else {
			digitalWrite(RELAY_PIN, LOW);
		}

		const auto data = DataPacket(temperature, 0, 0, 0);
		sendData(data);
		last_time_data_out_ = current_time;
	}
}

//change the state of the test based on the last received command
void TestEngine::onCommandReceived(const CommandPacket& command_packet) {
	last_received_command_ = command_packet;

	switch (command_packet.command) {
		case INIT:
			test_state_.changeState(State::IDLE);
			break;
		case START:
			test_specs_ = command_packet.test_spec_packet;
			test_state_.changeState(State::RUNNING);
			break;
		case STOP:
			test_state_.changeState(State::STOPPED);
			break;
		case BAD_COMMAND:
			test_state_.changeState(State::UNKNOWN);
			break;
		default:
			break;
	}
}

void TestEngine::setCommandCallback(const CommandCallBack& callback) {
	send_command_callback_ = callback;
}

void TestEngine::setDataCommandCallback(const CommandCallBack& callback) {
	send_data_command_callback_ = callback;
}

void TestEngine::onIdle() {
	digitalWrite(RELAY_PIN, LOW);
	pingBackCommand();
}

void TestEngine::onStart() {
	control_loop_.configure(test_specs_);
	temperature_sensor_.begin();
	pingBackCommand();
}

void TestEngine::onStop() {
	digitalWrite(RELAY_PIN, LOW);
	control_loop_.reset();
	pingBackCommand();
}

void TestEngine::onUnknown() {
	pingBackCommand();
}

void TestEngine::sendData(const DataPacket& data_packet) {
	const auto command = CommandPacket(DATA, DATA ^ 0xFF, data_packet);
	callbackDataCommand(command);
}

//sends command back to the application, so it knows the command worked
void TestEngine::pingBackCommand() {
	if(send_data_command_callback_) {
		send_command_callback_(last_received_command_);
	}
}

void TestEngine::callbackDataCommand(const CommandPacket& command_packet) {
	if(send_data_command_callback_) {
		send_data_command_callback_(command_packet);	
	}
}