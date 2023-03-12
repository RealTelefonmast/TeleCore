﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    internal static class CustomNetworkBillUtility
    {
        private static string repeatCountEditBuffer;
        private static string targetCountEditBuffer;

        public static void DrawDetails(Rect rect, CustomNetworkBill bill)
        {
            if (bill == null)
            {

                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            {
                var listing2 = listing.BeginSection(200);
                {
                    if (listing2.ButtonText(bill.RepeatMode.LabelCap))
                    {
                        bill.DoRepeatModeConfig();
                    }

                    if (bill.RepeatMode == BillRepeatModeDefOf.RepeatCount)
                    {
                        listing2.Label("RepeatCount".Translate(bill.repeatCount));
                        listing2.IntEntry(ref bill.repeatCount, ref repeatCountEditBuffer);
                    }
                    else if (bill.RepeatMode == BillRepeatModeDefOf.TargetCount)
                    {
                        string text = "CurrentlyHave".Translate() + ": ";
                        text += bill.CurrentCount;
                        text += " / ";
                        text += ((bill.targetCount < 999999) ? bill.targetCount.ToString() : "Infinite".Translate().ToLower().ToString());

                        /*
                        string str = bill.recipe.WorkerCounter.ProductsDescription(this.bill);
                        if (!str.NullOrEmpty())
                        {
                            text += "\n" + "CountingProducts".Translate() + ": " + str.CapitalizeFirst();
                        }
                        */
                        listing2.Label(text);
                        int targetCount = bill.targetCount;
                        listing2.IntEntry(ref bill.targetCount, ref targetCountEditBuffer);

                        Widgets.Dropdown(listing2.GetRect(30f), bill, (b) => b.includeFromZone, (b) => GenerateStockpileInclusion(bill), (bill.includeFromZone == null) ? "IncludeFromAll".Translate() : "IncludeSpecific".Translate(bill.includeFromZone.label));
                    }
                }
                listing.EndSection(listing2);
                listing.Gap(5);

                //StockPile
                Listing_Standard listing3 = listing.BeginSection(30, 4f, 4f);
                {
                    string text2 = string.Format(bill.StoreMode.LabelCap, (bill.StoreZone != null) ? bill.StoreZone.SlotYielderLabel() : "");
                    if (bill.StoreZone != null && !bill.CanPossiblyStoreInStockpile(bill.StoreZone))
                    {
                        text2 += $" ({"IncompatibleLower".Translate()})";
                        Text.Font = GameFont.Tiny;
                    }
                    if (listing3.ButtonText(text2, null))
                    {
                        bill.DoStoreModeConfig();
                    }
                    Text.Font = GameFont.Small;
                }
                listing.EndSection(listing3);
                listing.Gap(5);
            }
            listing.End();
        }

        //
        private static IEnumerable<Widgets.DropdownMenuElement<Zone_Stockpile>> GenerateStockpileInclusion(CustomNetworkBill bill)
        {
            yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
            {
                option = new FloatMenuOption("IncludeFromAll".Translate(), delegate { bill.includeFromZone = null; }),
                payload = null
            };
            List<SlotGroup> groupList = bill.billStack.ParentBuilding.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
            var groupCount = groupList.Count;
            int num;
            for (var i = 0; i < groupCount; i = num)
            {
                var slotGroup = groupList[i];
                if (slotGroup.parent is Zone_Stockpile stockpile)
                {
                    if (!bill.CanPossiblyStoreInStockpile(stockpile))
                    {
                        yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
                        {
                            option = new FloatMenuOption(
                                $"{"IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                                null),
                            payload = stockpile
                        };
                    }
                    else
                    {
                        yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
                        {
                            option = new FloatMenuOption(
                                "IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()),
                                delegate { bill.includeFromZone = stockpile; }),
                            payload = stockpile
                        };
                    }
                }
                num = i + 1;
            }
        }

        //
        public static int CountProducts(CustomNetworkBill bill)
        {
            ThingDefCountClass thingDefCountClass = bill.results[0];
            ThingDef thingDef = thingDefCountClass.thingDef;
            if (thingDefCountClass.thingDef.CountAsResource && bill.includeFromZone == null)
            {
                return bill.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) + GetCarriedCount(bill, thingDef);
            }
            int num = 0;
            if (bill.includeFromZone == null)
            {
                num = CountValidThings(bill.Map.listerThings.ThingsOfDef(thingDefCountClass.thingDef), bill, thingDef);
                if (thingDefCountClass.thingDef.Minifiable)
                {
                    List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
                    for (int i = 0; i < list.Count; i++)
                    {
                        MinifiedThing minifiedThing = (MinifiedThing)list[i];
                        if (CountValidThing(minifiedThing.InnerThing, bill, thingDef))
                        {
                            num += minifiedThing.stackCount * minifiedThing.InnerThing.stackCount;
                        }
                    }
                }
                num += GetCarriedCount(bill, thingDef);
            }
            else
            {
                foreach (Thing outerThing in bill.includeFromZone.AllContainedThings)
                {
                    Thing innerIfMinified = outerThing.GetInnerIfMinified();
                    if (CountValidThing(innerIfMinified, bill, thingDef))
                    {
                        num += innerIfMinified.stackCount;
                    }
                }
            }
            return num;
        }

        private static int GetCarriedCount(CustomNetworkBill bill, ThingDef prodDef)
        {
            int num = 0;
            foreach (Pawn pawn in bill.Map.mapPawns.FreeColonistsSpawned)
            {
                Thing thing = pawn.carryTracker.CarriedThing;
                if (thing != null)
                {
                    int stackCount = thing.stackCount;
                    thing = thing.GetInnerIfMinified();
                    if (CountValidThing(thing, bill, prodDef))
                    {
                        num += stackCount;
                    }
                }
            }
            return num;
        }

        public static int CountValidThings(List<Thing> things, CustomNetworkBill bill, ThingDef def)
        {
            int num = 0;
            for (int i = 0; i < things.Count; i++)
            {
                if (CountValidThing(things[i], bill, def))
                {
                    num++;
                }
            }
            return num;
        }

		public static bool CountValidThing(Thing thing, CustomNetworkBill bill, ThingDef def)
        {
            ThingDef def2 = thing.def;
            if (def2 != def)
            {
                return false;
            }
            if (def2.IsApparel && ((Apparel)thing).WornByCorpse)
            {
                return false;
            }
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            return (compQuality == null);
        }
    }
}
