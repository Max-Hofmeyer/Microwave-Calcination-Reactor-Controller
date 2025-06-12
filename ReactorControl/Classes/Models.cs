using System.Text.Json.Serialization;

namespace ReactorControl.Classes;

public class Models
{
    public enum ReactorCommandsEnum
    {
        Init,
        Start,
        Stop,
        Cooldown,
        Data,
        Debug,
        BadCommand,
        InternalError,
    }

    public enum TestState
    {
        Idle,
        Running,
        CoolingDown,
        Stopped,
        Unknown,
        Frozen,
        Disconnected,
    }

    public struct TestSpecPacket
    {
        public double TargetTemp { get; init; }
        public double DeltaTemp { get; init; }
        public double TargetHoldTime { get; init; }
        public double Emissivity { get; init; }
        public bool TestMode { get; init; }
    }

    public struct DataPacket
    {
        public double? TemperatureValue { get; init; }
        public double? WallPowerValue { get; init; }

        [JsonIgnore]
        public double TimeStamp { get; set; }
    }

    public struct CommandPacket
    {
        public ReactorCommandsEnum Command { get; init; }
        public TestSpecPacket? TestSpecs { get; init; }

        [JsonPropertyName("DataPacket")]
        public DataPacket? Data { get; init; }
        public string? DebugMessage { get; init; }

        [JsonIgnore]
        public bool? WithErrors { get; init; }
    }
}