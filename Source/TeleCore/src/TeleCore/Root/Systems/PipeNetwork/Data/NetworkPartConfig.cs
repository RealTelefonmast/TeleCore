﻿using System;
using System.Collections.Generic;
using TeleCore.FlowCore;
using TeleCore.Network.Data;
using TeleCore.Network.IO;
using Verse;

namespace TeleCore.Network;

public class NetworkPartConfig : Editable
{
    #region XML Fields

    public Type workerType = typeof(NetworkPart);
    public NetworkDef networkDef;
    public NetworkRole roles = NetworkRole.Transmitter;
    public bool requiresController;
    public NetIOConfig? netIOConfig;
    public FlowVolumeConfig<NetworkValueDef> volumeConfig;

    public float gasThroughPut = 0.03f;
    public float friction = 0.1f;

    public float CSquared => networkDef.gasThroughPutOverride ?? gasThroughPut;
    public float Friction => networkDef.frictionOverride ?? friction;

    public override IEnumerable<string> ConfigErrors()
    {
        if (volumeConfig == null)
            yield return $"A network part cannot have a null volume config! (NetworkDef: {networkDef})";
    }

    public void PostLoadSpecial(ThingDef parent)
    {
        TLog.Debug("PostLoadSpecial");
        netIOConfig?.PostLoadCustom(parent);
    }

    #endregion
}