﻿using System.Collections.Generic;
using RimWorld;
using TeleCore.Network;
using TeleCore.Network.Data;
using TeleCore.Static;
using Verse;
using Verse.AI;

namespace TeleCore;

public class WorkGiver_EmptyPortableContainers : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.ThingGroupCache().ThingsOfGroup(TeleDefOf.NetworkPortableContainers);
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return base.ShouldSkip(pawn, forced);
    }
    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced)) return false;
        if (t is PortableNetworkContainer container)
        {
            return container.HasValidTarget;
        }
        JobFailReason.Is($"\n{"TELE.PortableContainer.CannotStartEmptyJob".Translate()}", null);
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        PortableNetworkContainer networkContainer = t as PortableNetworkContainer;
        var job = new Job(TeleDefOf.EmptyPortableContainer, networkContainer, networkContainer.TargetToEmptyAt, networkContainer.TargetToEmptyAt.Cell);
        job.haulMode = HaulMode.Undefined;
        return job;
    }
}