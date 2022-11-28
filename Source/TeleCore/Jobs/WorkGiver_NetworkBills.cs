﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TeleCore.Static;
using Verse;
using Verse.AI;

namespace TeleCore
{
    public class WorkGiver_NetworkBills : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (def.fixedBillGiverDefs is {Count: 1})
                    return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);

                return ThingRequest.ForGroup(ThingRequestGroup.Undefined);
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (def.fixedBillGiverDefs != null) return base.PotentialWorkThingsGlobal(pawn);

            //
            return Targets(pawn.Map);
        }

        private IEnumerable<Thing> Targets(Map map)
        {
            return map.TeleCore().ThingGroupCacheInfo.ThingsOfGroup(ThingGroupDefOf.NetworkBillCrafters);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is ThingWithComps thing && !thing.IsPoweredOn()) return false;

            var compTNW = t.TryGetComp<Comp_NetworkBillsCrafter>();
            if (compTNW == null) return false;
            if (compTNW.BillStack.Count == 0) return false;
            if (compTNW.BillStack.ParentNetParts.Any(t => !t.Network.IsWorking)) return false;
            if (compTNW.billStack.CurrentBill != null)
            {
                if (!compTNW.billStack.CurrentBill.ShouldDoNow()) return false;
                return !t.IsReserved(pawn.Map, out _);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            return new Job(TeleDefOf.DoNetworkBill, thing);
        }
    }
}
