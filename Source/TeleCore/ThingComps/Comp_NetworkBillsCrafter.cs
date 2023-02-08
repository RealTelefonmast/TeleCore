﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class Comp_NetworkBillsCrafter : Comp_NetworkStructure
    {
        public new Building_WorkTable parent;
        public NetworkBillStack billStack;

        //CompFX
        public Color CurColor => Color.clear;

        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => IsWorkedOn,
                _ => base.FX_ShouldDraw(args),
            };
        }

        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => CurColor,
                _ => base.FX_GetColor(args),
            };
        }

        public override bool? FX_ShouldThrowEffects(FXLayerArgs args)
        {
            return IsWorkedOn;
        }

        //Crafter Code
        public new CompProperties_NetworkBillsCrafter Props => (CompProperties_NetworkBillsCrafter)base.Props;

        public bool IsWorkedOn => BillStack.CurrentBill != null;
        public NetworkBillStack BillStack => billStack;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent = base.parent as Building_WorkTable;
            if (!respawningAfterLoad)
                billStack = new NetworkBillStack(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref billStack, "tiberiumBillStack", this);
        }
    }
}
