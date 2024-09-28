#include "CommunicationHandler.h"
#include "TestEngine.h"

#define RELAY_PIN 32

CommunicationHandler communication_handler;
TestEngine test_engine;

//when the command received event is raised (by the CommunicationHandler), route it to the test engine
void onCommandReceived(const CommandPacket& command_packet) {
	test_engine.onCommandReceived(command_packet);
}

//when a command requested event is raised (by the TestEngine), route it to the communication manager
void onCommandRequested(const CommandPacket& command_packet) {
    communication_handler.sendCommand(command_packet);
}

//when a data ready event is raised (by the TestEngine), route it to the communication manager
void onDataReady(const CommandPacket& data_packet) {
    communication_handler.pushData(data_packet);
}

void setup(){
    Serial.begin(115200);
    Serial2.begin(9600, SERIAL_8N1, RX, TX);
    pinMode(RELAY_PIN, OUTPUT);
    digitalWrite(RELAY_PIN, LOW);
    
    communication_handler.setCommandReceivedCallback(onCommandReceived);
    test_engine.setCommandCallback(onCommandRequested);
    test_engine.setDataCommandCallback(onDataReady);
}

void loop()
{
	communication_handler.update();
    test_engine.update();
}
