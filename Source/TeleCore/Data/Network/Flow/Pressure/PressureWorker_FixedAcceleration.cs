﻿namespace TeleCore.Network.Flow.Pressure;

public class PressureWorker_FixedAcceleration : PressureWorker
{
    public override string Description =>
        "Fixed acceleration based on sign of pressure difference (as proposed by Quinor)";

    public override double CSquared { get; }
    public override double Friction { get; }

    public double Acceleration => 5;
    public double Inertia => 0.9;

    public override double FlowFunction(NetworkVolume t0, NetworkVolume t1, double f)
    {
        f *= Inertia;
        f += (PressureFunction(t0) - PressureFunction(t1) > 0 ? 1 : -1) * Acceleration;
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}