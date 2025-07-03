using ReactorControl.Types;

namespace ReactorControl.Classes;

public class StateStore
{
    private readonly object _lock = new();
    private DeviceState _state = new DeviceState();
    private DeviceData _data = new DeviceData();

    public void Update(DeviceState state, DeviceData data) {
        lock (_lock) {
            _state = state;
            _data = data;
        }
    }

    public (DeviceState State, DeviceData Data) Snapshot() {
        lock (_lock) {
            return (_state, _data);
        }
    }
}