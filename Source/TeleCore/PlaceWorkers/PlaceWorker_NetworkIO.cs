﻿using UnityEngine;
using Verse;

namespace TeleCore;

public class PlaceWorker_NetworkIO : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var network = def.GetCompProperties<CompProperties_Network>();

        if (network.generalIOPattern != null)
        {
            network.SimpleIO.Draw(center, def, rot);
            return;
        }
        
        foreach (var part in network.networks)
        {
            part.SimpleIO.Draw(center, def, rot);
        }
    }
}