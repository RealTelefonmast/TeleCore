﻿using TeleCore.FlowCore;
using TeleCore.Network.Data;

namespace TeleCore.Network.Flow.Pressure;

public class PressureWorker_WaveEquationNonLinear : PressureWorker
{
    public override string Description => "Wave equation with non-linear pressure.";

    public override double Friction => 0.001;
    public override double CSquared => 0.01;

    public override double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double f)
    {
        NetworkVolume from = iface.From;
        NetworkVolume to = iface.To;

        f += (PressureFunction(from) - PressureFunction(to)) * CSquared;
        f *= 1 - Friction;
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        var p = t.TotalValue / t.MaxCapacity * 100;
        return p <= 60 ? p : 60 + (p - 60) * 10;
    }
}