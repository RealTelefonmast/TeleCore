﻿using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore;

public class Graphic_LinkedWithSame : Graphic_Linked
{
    public Graphic_LinkedWithSame()
    {
    }

    public Graphic_LinkedWithSame(Graphic subGraphic)
    {
        this.subGraphic = subGraphic;
    }

    public override Material MatSingle =>
        MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_LinkedWithSame(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }

    public override void Init(GraphicRequest req)
    {
        subGraphic =
            GraphicDatabase.Get<Graphic_Single>(req.path, req.shader, Vector2.one, Color.white, Color.white, data);
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        var sameThing = c.GetFirstThing(parent.Map, parent.def);
        return c.InBounds(parent.Map) && sameThing != null;
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        var mat = LinkedDrawMatFrom(thing, thing.Position);
        Printer_Plane.PrintPlane(layer, thing.TrueCenter(), new Vector2(1f, 1f), mat, extraRotation);
    }
}