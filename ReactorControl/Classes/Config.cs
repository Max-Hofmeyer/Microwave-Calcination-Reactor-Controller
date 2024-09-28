using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorControl.Classes;

public class Config
{
    public decimal MaxTargetTemperature { get; init; }
    public decimal MinTargetTemperature { get; init; }
           
    public decimal MaxDeltaTemperature { get; init; }
    public decimal MinDeltaTemperature { get; init; }

    public int MaxTargetHoldTime { get; init; }
    public int MinTargetHoldTime { get; init; }
}