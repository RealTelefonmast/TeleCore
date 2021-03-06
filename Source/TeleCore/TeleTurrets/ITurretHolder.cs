using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace TeleCore
{
    public interface ITurretHolder
    {
        LocalTargetInfo TargetOverride { get; }
        bool IsActive { get; }
        bool PlayerControlled { get; }

        Thing Caster { get; }
        Thing HolderThing { get; }
        Faction Faction { get; }

        //
        CompPowerTrader PowerComp { get; }
        CompCanBeDormant DormantComp { get; }
        CompInitiatable InitiatableComp { get; }
        CompMannable MannableComp { get; }
        // 
        CompRefuelable RefuelComp { get; }
        Comp_NetworkStructure NetworkComp { get; }
        StunHandler Stunner { get; }

        void Notify_OnProjectileFired();
        bool ThreatDisabled(IAttackTargetSearcher disabledFor);
    }
}
