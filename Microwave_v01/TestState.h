// TestEngine.h

#ifndef _TESTSTATE_h
#define _TESTSTATE_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif
#include <functional>
#include <utility>
#include "models.h"

class TestState {
public:
	TestState() : current_state_(State::IDLE), on_idle_(nullptr), on_start_(nullptr), on_cooldown_(nullptr), on_shutdown_(nullptr), on_unknown_(nullptr) {}

    void setInitCallback(std::function<void()> call_back);
    void setRunningCallback(std::function<void()> call_back);
	void setCooldownCallback(std::function<void()> call_back);
    void setShutdownCallback(std::function<void()> call_back);
	void setUnknownCallback(std::function<void()> call_back);

	void changeState(State new_state);
	void handleStateChange() const;
	State getCurrentState() const;

private:
	State current_state_;
	std::function<void()> on_idle_, on_start_, on_cooldown_, on_shutdown_, on_unknown_;
};

#endif

