using System;

namespace TeleCore.FlowCore.Flow.Pressure;

public abstract class PressureWorker
{
    public abstract string Description { get; }

    public abstract double CSquared { get; }
    public abstract double Friction { get; }


    public virtual DefValueStack<Values.NetworkValueDef, double> FlowFunction(
        FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef> iface, DefValueStack<Values.NetworkValueDef, double> f)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public abstract double FlowFunction(FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef> iface, double f);

    public abstract double PressureFunction(NetworkVolume t);

    //TODO: Maybe move to utility class?
    public static double GetTotalFriction(NetworkVolume volume)
    {
        double totalFriction = 0;
        double totalVolume = 0;

        if (!volume.Stack.IsValid) return 0;
        foreach (var fluid in volume.Stack)
        {
            totalFriction += fluid.Def.friction * fluid.Value;
            totalVolume += fluid.Value;
        }

        if (totalVolume == 0) return 0;

        var averageFriction = totalFriction / totalVolume;
        return averageFriction;
    }

}