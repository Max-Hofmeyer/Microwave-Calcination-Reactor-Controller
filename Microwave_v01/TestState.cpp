#include "TestState.h"

void TestState::setInitCallback(std::function<void()> call_back) {
    on_idle_ = std::move(call_back);
}

void TestState::setRunningCallback(std::function<void()> call_back) {
    on_start_ = std::move(call_back);
}

void TestState::setCooldownCallback(std::function<void()> call_back) {
    on_cooldown_ = std::move(call_back);
}

void TestState::setShutdownCallback(std::function<void()> call_back) {
    on_shutdown_ = std::move(call_back);
}

void TestState::setUnknownCallback(std::function<void()> call_back) {
    on_unknown_ = std::move(call_back);
}

void TestState::changeState(State new_state) {
    current_state_ = new_state;
    handleStateChange();
}

void TestState::handleStateChange() const {
    switch (current_state_) {
    case State::IDLE:
        if (on_idle_) on_idle_();
        break;
    case State::RUNNING:
        if (on_start_) on_start_();
        break;
    case State::COOLDOWN:
        if (on_cooldown_) on_cooldown_();
        break;
    case State::STOPPED:
        if (on_shutdown_) on_shutdown_();
        break;
    case State::UNKNOWN:
        if (on_unknown_) on_unknown_();
        break;
    }
}

State TestState::getCurrentState() const {
	return current_state_;
}
