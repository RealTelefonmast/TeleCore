﻿using System;

namespace TeleCore.Network.Flow.Pressure;

public class PressureWorker_WaveEquationDamping2 : PressureWorker
{
    public override string Description => "Model that applies additional friction when waves occur.";

    public override double Friction => 0;
    public override double CSquared => 0.03;
    public double DampFriction => 0.01;

    public override double FlowFunction(NetworkVolume from, NetworkVolume to, double f)
    {
        var dp = PressureFunction(from) - PressureFunction(to);
        var counterFlow = Math.Sign(f) != Math.Sign(dp);
        f += dp * CSquared;
        f *= 1 - Friction;
        if (counterFlow) f *= 1 - Math.Min(0.9, DampFriction * Math.Abs(dp) * 0.01);
        return f;
    }

    public override double PressureFunction(NetworkVolume t)
    {
        return t.TotalValue / t.MaxCapacity * 100;
    }
}