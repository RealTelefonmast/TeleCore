﻿using System.Collections.Generic;
using TeleCore;
using TeleCore.FlowCore;
using Verse;

namespace TeleCore;

public class NetworkValueDef : FlowValueDef
{
    //
    public ThingDef? SpecialDroppedContainerDef = null;
    public ThingDef? ThingDroppedFromContainer = null;
    public float ValueToThingRatio = 1;

    public NetworkValueDef(){}
    
    public NetworkValueDef(ThingDef? thingDroppedFromContainer)
    {
        ThingDroppedFromContainer = thingDroppedFromContainer;
    }

    public NetworkDef NetworkDef => (NetworkDef)collectionDef;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
        {
            yield return configError;
        }
    }
}