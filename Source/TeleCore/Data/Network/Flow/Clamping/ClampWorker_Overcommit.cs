﻿namespace TeleCore.Network.Flow.Clamping;

public class ClampWorker_Overcommit : ClampWorker
{
    public override string Description => "Limit flow to a configurable fraction of current content (outflow) or remaining space (inflow)";

    public override bool EnforceMinPipe => true;
    public override bool EnforceMaxPipe => true; 
    public override bool MaintainFlowSpeed => false;
    public override double MinDivider => 4;
    public override double MaxDivider => 1;

    public override double ClampFunction(FlowBox t0, FlowBox t1, double f, ClampType type) 
    {  
        double d, c, r;
        if (EnforceMinPipe) 
        {
            // Limit outflow to 1/divider of fluid content in src pipe     
            if (type == ClampType.FlowSpeed && MaintainFlowSpeed) 
            {
                d = 1;
            }
            else 
            {
                d = 1 / MinDivider;
            }
            if (f > 0) 
            {
                c = t0.TotalValue;
                f = ClampFlow(c, f, d*c);
            } 
            else if (f < 0) 
            {
                c = t1.TotalValue;
                f = -ClampFlow(c, -f, d*c);
            }
        }
        if (EnforceMaxPipe && (type == ClampType.FluidMove || !MaintainFlowSpeed)) 
        {
            // Limit inflow to 1/divider of remaining space in dst pipe
            d = 1 / MaxDivider;
            if (f > 0)
            {
                r = t1.MaxCapacity - t1.TotalValue;
                f = ClampFlow(r, f, d*r);
            } 
            else if (f < 0)
            {
                r = t0.MaxCapacity - t0.TotalValue;
                f = -ClampFlow(r, -f, d*r);
            }
        }
        return f;
    }
}