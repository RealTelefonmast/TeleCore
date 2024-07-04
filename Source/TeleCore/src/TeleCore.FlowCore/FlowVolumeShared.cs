using System;

namespace TeleCore.FlowCore;

public class FlowVolumeShared<T> : FlowVolumeBase<T> where T : FlowValueDef
{
    public override double MaxCapacity => _config.capacity * AllowedValues.Count;

    public override double CapacityOf(T? def)
    {
        return _config.capacity;
    }

    public override bool IsFull(T def)
    {
        return StoredValueOf(def) >= CapacityOf(def);
    }

    protected override double ExcessFor(T def, double amount)
    {
        return Math.Max(StoredValueOf(def) + amount - CapacityOf(def), 0); ;
    }

    public FlowVolumeShared(FlowVolumeConfig<T> config) : base(config)
    {
    }
}
