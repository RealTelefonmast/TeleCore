﻿using System.Collections.Generic;
using TeleCore.FlowCore;
using TeleCore.Network.Data;
using TeleCore.Primitive;

namespace TeleCore.Network.Flow;

/// <summary>
///     The logical handler for fluid flow.
///     Area and height define the total content, elevation allows for flow control.
/// </summary>
public class NetworkVolume : FlowVolume<NetworkValueDef>
{
    private readonly FlowVolumeConfig _config;

    public NetworkVolume(FlowVolumeConfig config)
    {
        _config = config;
    }

    public override double MaxCapacity => _config.Volume;

    public double FillHeight => TotalValue / MaxCapacity * _config.height;

    //TODO => Move into container config
    public IList<NetworkValueDef> AcceptedTypes { get; set; }

    public FlowResult<NetworkValueDef, double>  TryAdd(NetworkValueDef def, double value)
    {
        mainStack += (def, value);
        return FlowResult<NetworkValueDef, double>.Init(value).Complete((def, value)).Resolve();
    }

    public FlowResult<NetworkValueDef, double> TryRemove(NetworkValueDef def, double value)
    {
        mainStack -= (def, value);
        return FlowResult<NetworkValueDef, double>.Init(-value).Complete((def, -value));
    }

    public FlowResult<NetworkValueDef, double>  TryConsume(NetworkValueDef def, double value)
    {
        return TryRemove(def, value);
    }

    public void Clear()
    {
    }
}