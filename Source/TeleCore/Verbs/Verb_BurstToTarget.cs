﻿using Verse;

namespace TeleCore;

public class Verb_BurstToTarget : Verb_ProjectileExtended
{
    protected override bool TryCastAttack()
    {
        if (!currentTarget.IsValid) return false;
        var from = DrawPos.ToIntVec3();
        var to = currentTarget.Cell;
        var distance = from.DistanceTo(to);
        if (distance < Props.range)
        {
            var normed = (to - from).ToVector3().normalized;
            var newTo = from + (normed * Props.range).ToIntVec3();
            to = newTo;
        }

        var line = new ShootLine(from, to);
        foreach (var cell in line.Points())
        {
            if (cell.DistanceTo(from) <= Props.minRange) continue;
            var line2 = new ShootLine(from, cell);
            AdjustedTarget(cell, ref line2, out var flags);
            CastProjectile(from, caster, CurrentStartPos, cell, currentTarget, flags, false, null, null);
        }

        return true;
    }
}