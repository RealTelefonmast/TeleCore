﻿using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    /// <summary>
    /// A default placeworker for network pipes, uses the ThingDef.uiIconPath as the ghost render texture.
    /// </summary>
    public class PlaceWorker_Pipe : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var comp = loc.GetThingList(map).Select(t => t.TryGetComp<Comp_Network>()).FirstOrDefault();
            if (comp is null) return true;

            var networks = ((checkingDef as ThingDef)?.comps.Find(c => c is CompProperties_Network) as CompProperties_Network)?.networks?.Select(n => n.networkDef).ToArray();
            if (comp.NetworkParts.Select(t => t.NetworkDef).Any(networks.Contains))
            {
                return false;
            }
            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            int num = 0;
            num = Gen.HashCombine(num, def.graphicData);
            num = Gen.HashCombine(num, def);
            num = Gen.HashCombineStruct(num, ghostCol);
            if (!GhostUtility.ghostGraphics.TryGetValue(num, out var graphic))
            {
                graphic = GraphicDatabase.Get<Graphic_Single>(def.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader, def.graphicData.drawSize, ghostCol);
                GhostUtility.ghostGraphics.Add(num, graphic);
            }
            Vector3 loc = GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.Blueprint.AltitudeFor());
            graphic.DrawFromDef(loc, rot, def, 0f);
        }
    }
}
