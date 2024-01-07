﻿using UnityEngine;
using Verse;

namespace TeleCore;

public class MoteBeam : TeleMote
{
    protected float width;
    protected MoteAttachLink link2 = MoteAttachLink.Invalid;

    public void Attach(TargetInfo a, TargetInfo b)
    {
        link1 = new MoteAttachLink(a, Vector3.zero);
        link2 = new MoteAttachLink(b, Vector3.zero);
    }

    public void Attach(TargetInfo a, TargetInfo b, Vector3 offsetA, Vector3 offsetB)
    {
        link1 = new MoteAttachLink(a, offsetA);
        link2 = new MoteAttachLink(b, offsetB);
    }

    
    public void UpdateWidth(float width)
    {
        this.width = width;
    }
    
    public override void Draw()
    {
        UpdatePositionAndRotation();
        base.Draw();
    }

    public void UpdateTargets(TargetInfo a, TargetInfo b, Vector3 offsetA, Vector3 offsetB)
    {
        link1.UpdateTarget(a, offsetA);
        link2.UpdateTarget(b, offsetB);
    }

    protected void UpdatePositionAndRotation()
    {
        if (link1.Linked)
        {
            if (link2.Linked)
            {
                if (!link1.Target.ThingDestroyed) link1.UpdateDrawPos();
                if (!link2.Target.ThingDestroyed) link2.UpdateDrawPos();
                exactPosition = (link1.LastDrawPos + link2.LastDrawPos) * 0.5f;
                if (def.mote.rotateTowardsTarget)
                    exactRotation = link1.LastDrawPos.AngleToFlat(link2.LastDrawPos) + 90f;
                if (def.mote.scaleToConnectTargets)
                    exactScale = new Vector3(width, 1, (link2.LastDrawPos - link1.LastDrawPos).MagnitudeHorizontal());
            }
            else
            {
                if (!link1.Target.ThingDestroyed) link1.UpdateDrawPos();
                exactPosition = link1.LastDrawPos + def.mote.attachedDrawOffset;
            }
        }

        exactPosition.y = def.altitudeLayer.AltitudeFor();
    }
    
    
    private Vector3 end;
    private Vector3 start;
    public void Draw2()
    {
        if (AttachedMat == null) return;
        materialProps ??= new MaterialPropertyBlock();

        var alpha = Alpha;
        /*if (shouldMove && AgeSecs >= props.mote.fadeInTime)
            end2 = Vector3.Lerp(puller, finalEnd, alpha);
        */
        var diff = end - start;
        if (alpha <= 0f) return;
        var color = instanceColor;
        color.a *= alpha;
        materialProps.SetColor("_Color", color);

        var z = diff.MagnitudeHorizontal();
        var pos = (start + end) / 2f;
        pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var scale = new Vector3(1f, 1f, z);
        var quat = Quaternion.LookRotation(diff);
        Matrix4x4 matrix = default;
        matrix.SetTRS(pos, quat, scale);
        Graphics.DrawMesh(MeshPool.plane10, matrix, AttachedMat, 0, null, 0, materialProps);
    }
}