﻿using System;

namespace TeleCore.FlowCore.Flow.Clamping;

public class ClampWorker_ConnectionCountLimit : ClampWorker
{
    private readonly NetworkFlowSystem _parentFlowSystem;

    public ClampWorker_ConnectionCountLimit(NetworkFlowSystem parentFlowSystem)
    {
        _parentFlowSystem = parentFlowSystem;
    }

    public override string Description =>
        "Limit flow to (1/connections) of current content (outflow) or remaining space (inflow)";

    public override bool EnforceMinPipe => true;
    public override bool EnforceMaxPipe => true;
    public override bool MaintainFlowSpeed => false;
    public override double MinDivider => 1;
    public override double MaxDivider => 1;

    public override double ClampFunction(FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef> iface, double f, ClampType type)
    {
        var d0 = 1d / Math.Max(1, _parentFlowSystem.Connections[iface.From].Count);
        var d1 = 1d / Math.Max(1, _parentFlowSystem.Connections[iface.To].Count);
        double c, r;
        if (EnforceMinPipe)
        {
            if (f > 0)
            {
                c = iface.From.TotalValue;
                f = ClampFlow(c, f, d0 * c);
            }
            else if (f < 0)
            {
                c = iface.To.TotalValue;
                f = -ClampFlow(c, -f, d1 * c);
            }
        }

        if (EnforceMaxPipe)
        {
            if (f > 0)
            {
                r = iface.To.MaxCapacity - iface.To.TotalValue;
                f = ClampFlow(r, f, d1 * r);
            }
            else if (f < 0)
            {
                r = iface.From.MaxCapacity - iface.From.TotalValue;
                f = -ClampFlow(r, -f, d0 * r);
            }
        }

        return f;
    }
}