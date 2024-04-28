﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Network.Data;
using TeleCore.Network.Utility;
using TeleCore.Primitive;
using TeleCore.Static;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.Network.Bills;

/// <summary>
/// Payment part
/// </summary>
public partial class CustomNetworkBill
{
    private DefValueStack<NetworkValueDef, double> _cost;
    private bool _hasBeenPaid;

    public bool HasBeenPaid => _hasBeenPaid;
    public bool CanBePaid => !_hasBeenPaid && CanPay;
    private bool CanPay
    {
        get
        {
            if (_cost.IsEmpty)
            {
                TLog.Error($"Trying to pay for {billName} with empty networkCost! | Paid: {HasBeenPaid} WorkLeft: {WorkLeft}");
                return false;
            }

            double debt = _cost.TotalValue;
            var available = TotalAvailable;
            foreach (var value in _cost)
            {
                if (available[value.Def] > value.Value)
                {
                    debt -= value.Value;
                }
            }
            return debt == 0f;
        }
    }

    public DefValueStack<NetworkValueDef, double> Cost => _cost;

    //TODO: Consider Caching?
    /// <summary>
    /// Relevant networks for the current cost
    /// </summary>
    public IEnumerable<NetworkDef> PaymentNetworkSources
    {
        get
        {
            return _cost.Values.Select(defValue => defValue.Def.NetworkDef).Distinct();
        }
    }

    public IEnumerable<INetworkPart> AvailableSourceParts
    {
        get
        {
            var parentComp = _ownerStack.ParentComp;
            foreach (var sourceDef in PaymentNetworkSources)
            {
                var sourcePart = parentComp[sourceDef];
                if (sourcePart.HasContainer)
                {
                    //Return local part first
                    yield return sourcePart;
                }
                else
                {
                    //If local part has no container, check if it has logical edges
                    if (sourcePart.Network.Graph.TryGetAdjacencyList(sourcePart, out var adjParts))
                    {
                        foreach (var adjData in adjParts)
                        {
                            if (adjData.Edge.IsLogical && adjData.Node.Value.HasContainer)
                            {
                                yield return adjData.Node.Value;
                            }
                        }
                    }
                }
            }
        }
    }

    public DefValueStack<NetworkValueDef, double> TotalAvailable
    {
        get
        {
            var stack = new DefValueStack<NetworkValueDef, double>();
            foreach (var part in AvailableSourceParts)
            {
                stack += part.Volume.Stack;
            }
            return stack;
        }
    }

    private void ExposePayment()
    {
        Scribe_Deep.Look(ref _cost, "cost");
        Scribe_Values.Look(ref _hasBeenPaid, "hasBeenPaid");
    }

    public void SetCost(DefValueStack<NetworkValueDef, double> costStack)
    {
        _cost = costStack;
    }

    public bool TryPay()
    {
        var stack = _cost;

        //Shouldnt be necessary due to immutability
        // foreach (var value in _cost)
        // {
        //     stack += new DefValue<NetworkValueDef, double>(value.Def, value.Value);
        // }

        foreach (var part in AvailableSourceParts)
        {
            var volume = part.Volume;
            foreach (var value in stack)
            {
                var remResult = volume.TryRemove(value.Def, value.Value);
                if (volume.StoredValueOf(value.Def) > 0 && remResult)
                {
                    stack -= (value.Def, remResult.Actual);
                }

                if (stack.TotalValue <= 0)
                {
                    _hasBeenPaid = true;
                    if (_cost.TotalValue <= 0)
                    {
                        TLog.Error("TOTALCOST WAS MUTATED!");
                    }
                    return true;
                }
            }
        }

        if (stack.TotalValue > 0d)
            TLog.Error($"TotalCost higher than 0 after payment! LeftOver: {stack.TotalValue}");
        return false;
    }

    private void TryRefund()
    {
        if (HasBeenPaid)
        {
            Refund();
        }
    }

    private void Refund()
    {
        foreach (var netComp in AvailableSourceParts)
        {
            var portableDef = netComp.Config.networkDef.portableContainerDef;
            if (portableDef == null)
            {
                Messages.Message(Translations.Messages.NoPortableContainer(netComp), MessageTypeDefOf.RejectInput, false);
                continue;
            }
            var newStack = StackFor(netComp);
            if (newStack.TotalValue > 0d)
                TLog.Warning($"Stack not empty ({newStack.TotalValue}) after refunding... dropping container.");
            //TODO: Refund portable container
            //GenPlace.TryPlaceThing(PortableNetworkContainer.CreateFromStack(portableDef, newStack), billStack.ParentBuilding.Position, billStack.ParentBuilding.Map, ThingPlaceMode.Near);
        }
    }

    private void ResetPayment()
    {
        _hasBeenPaid = false;
    }

    private DefValueStack<NetworkValueDef, double> StackFor(INetworkPart comp)
    {
        return DefValueStack<NetworkValueDef, double>.Empty;

        /*var storages = comp.ContainerSet[NetworkFlags];
        DefValueStack<NetworkValueDef> stack = new DefValueStack<NetworkValueDef>();
        foreach (var value in networkCost)
        {
            if(value.Def.NetworkDef == comp.Config.networkDef)
                stack += (value.Def, value.Value);
        }

        foreach (var storage in storages)
        {
            foreach (var value in stack)
            {
                var addReslt = storage.TryAddValue(value.Def, value.Value);
                if (addReslt)
                {
                    stack -= (value.Def, addReslt.ActualAmount);
                }
            }
        }
        return stack;*/
    }
}

/// <summary>
/// Rendering Part
/// </summary>
public partial class CustomNetworkBill
{
    private static readonly float borderWidth = 5;
    private static float contentHeight;

    private string WorkLabel => "TELE.NetworkBill.WorkLabel".Translate((int)workAmountLeft);
    private string CostLabel => "TELE.NetworkBill.CostLabel".Translate(NetworkBillUtility.CostLabel(Cost));

    public float DrawHeight
    {
        get
        {
            float height = 0;
            var labelSize = Text.CalcSize(billName);
            height += labelSize.y;

            float resultListHeight = (24 + 5) * results.Count;
            var labelHeight = labelSize.y * 2;
            height += contentHeight = labelHeight > resultListHeight ? labelHeight : resultListHeight;
            height += borderWidth * 2 + 30;
            return height;
        }
    }

    public void DrawBill(Rect rect, int index)
    {
        if (RepeatMode == BillRepeatModeDefOf.TargetCount && CurrentCount > targetCount)
            TWidgets.DrawHighlightColor(rect, TColor.Orange);

        if (!CanBePaid)
            TWidgets.DrawHighlightColor(rect, Color.red);

        if (HasBeenPaid)
            TWidgets.DrawHighlightColor(rect, Color.green);

        if (index % 2 == 0)
            Widgets.DrawAltRect(rect);

        rect = rect.ContractedBy(5);
        Widgets.BeginGroup(rect);
        {
            rect = rect.AtZero();

            //Name
            var labelSize = Text.CalcSize(billName);
            var labelRect = new Rect(new Vector2(0, 0), labelSize);
            Widgets.Label(labelRect, billName);

            //Controls
            var removeRect = new Rect(rect.width - 20f, 0f, 22f, 22f);
            var copyRect = new Rect(removeRect.x - 20, 0f, 22f, 22f);
            if (Widgets.ButtonImage(removeRect, TexButton.Delete, Color.white, Color.white * GenUI.SubtleMouseoverColor))
            {
                Destroy();
            }
            if (Widgets.ButtonImageFitted(copyRect, TeleContent.Copy, Color.white))
            {
                ClipBoardUtility.TrySetClipBoard(StringCache.NetworkBillClipBoard, Clone());
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }

            var newRect = new Rect(0, labelRect.height, rect.width, contentHeight);
            var leftRect = newRect.LeftHalf();
            var rightRect = newRect.RightHalf();

            //LEFT
            Widgets.BeginGroup(leftRect);
            {
                //List
                float curY = 0;
                foreach (var result in results)
                {
                    var row = new WidgetRow(0, curY, UIDirection.RightThenDown);
                    row.Icon(result.ThingDef.uiIcon, result.ThingDef.description);
                    row.Label($"×{result.Count}");
                    curY += 24 + 5;
                }
            }
            Widgets.EndGroup();

            //RIGHT
            Widgets.BeginGroup(rightRect);
            {
                var workBarRect = new Rect(rightRect.width - 75, rightRect.height - (24 + 5), 100, 24);
                Widgets.FillableBar(workBarRect, Mathf.InverseLerp(0, workAmountTotal, workAmountTotal - workAmountLeft));
            }
            Widgets.EndGroup();

            var bottomRect = new Rect(0, newRect.yMax, rect.width, 24);
            Widgets.BeginGroup(bottomRect);
            {
                bottomRect = bottomRect.AtZero();

                var countLabelSize = Text.CalcSize(CountLabel);
                var countLabelRect = new Rect(0, 0, countLabelSize.x, countLabelSize.y);
                Widgets.Label(countLabelRect, CountLabel);

                var controlRow = new WidgetRow();
                controlRow.Init(bottomRect.xMax, 0, UIDirection.LeftThenUp);
                if (controlRow.ButtonText("Details".Translate() + "..."))
                    _ownerStack.RequestDetails(this);
                if (controlRow.ButtonText(RepeatMode.LabelCap))
                    DoRepeatModeConfig();

                if (RepeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    var incrementor = 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                        incrementor = 10;
                    if (Input.GetKey(KeyCode.LeftControl))
                        incrementor = 100;

                    //
                    if (controlRow.ButtonIcon(TeleContent.Plus)) repeatCount += incrementor;
                    if (controlRow.ButtonIcon(TeleContent.Minus))
                        repeatCount = Mathf.Clamp(repeatCount - incrementor, 0, int.MaxValue);
                }

                if (RepeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    var incrementor = 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                        incrementor = 10;
                    if (Input.GetKey(KeyCode.LeftControl))
                        incrementor = 100;

                    //
                    if (controlRow.ButtonIcon(TeleContent.Plus)) targetCount += incrementor;
                    if (controlRow.ButtonIcon(TeleContent.Minus))
                        targetCount = Mathf.Clamp(targetCount - incrementor, 0, int.MaxValue);
                }
            }
            Widgets.EndGroup();
        }
        Widgets.EndGroup();
    }

    public void DoRepeatModeConfig()
    {
        var list = new List<FloatMenuOption>();
        list.Add(new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap,
            delegate { RepeatMode = BillRepeatModeDefOf.RepeatCount; }));
        list.Add(new FloatMenuOption(BillRepeatModeDefOf.TargetCount.LabelCap, delegate
        {
            /*
            if (!recipe.WorkerCounter.CanCountProducts(bill))
            {
                Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }
            */
            RepeatMode = BillRepeatModeDefOf.TargetCount;
        }));
        list.Add(new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap,
            delegate { RepeatMode = BillRepeatModeDefOf.Forever; }));
        Find.WindowStack.Add(new FloatMenu(list));
    }

    public void DoStoreModeConfig()
    {
        Text.Font = GameFont.Small;
        var list = new List<FloatMenuOption>();
        foreach (var billStoreModeDef in from bsm in DefDatabase<BillStoreModeDef>.AllDefs
                                         orderby bsm.listOrder
                                         select bsm)
            if (billStoreModeDef == BillStoreModeDefOf.SpecificStockpile)
            {
                List<SlotGroup> allGroupsListInPriorityOrder =
                    _ownerStack.ParentBuilding.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
                var count = allGroupsListInPriorityOrder.Count;
                for (var i = 0; i < count; i++)
                {
                    var group = allGroupsListInPriorityOrder[i];
                    if (group.parent is Zone_Stockpile stockpile)
                    {
                        //!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(this.bill, stockpile)
                        if (!CanPossiblyStoreInStockpile(stockpile))
                            list.Add(new FloatMenuOption(
                                $"{string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                                null));
                        else
                            list.Add(new FloatMenuOption(
                                string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel()),
                                delegate
                                {
                                    SetStoreMode(BillStoreModeDefOf.SpecificStockpile, (Zone_Stockpile)group.parent);
                                }));
                    }
                }
            }
            else
            {
                list.Add(new FloatMenuOption(billStoreModeDef.LabelCap, delegate { SetStoreMode(billStoreModeDef); }));
            }

        Find.WindowStack.Add(new FloatMenu(list));
    }
}

public partial class CustomNetworkBill : IExposable
{
    //SubSystems
    private NetworkBillStack _ownerStack;

    //
    public string billName;

    //General
    public DefValueStack<NetworkValueDef, double> byProducts;
    public Zone_Stockpile includeFromZone;
    public int repeatCount = -1;

    public List<ThingDefCount> results = new();

    public int targetCount = 1;
    private float workAmountLeft;
    public float workAmountTotal;

    public NetworkBillStack Stack => _ownerStack;

    private static NetworkRole NetworkFlags => NetworkRole.Storage | NetworkRole.Producer;

    public Map Map => _ownerStack.ParentBuilding.Map;
    public float WorkLeft => workAmountLeft;

    public int CurrentCount => CustomNetworkBillUtility.CountProducts(this);

    public string CountLabel
    {
        get
        {
            if (RepeatMode == BillRepeatModeDefOf.Forever)
                return "Forever.";
            if (RepeatMode == BillRepeatModeDefOf.RepeatCount)
                return $"{repeatCount}x";
            if (RepeatMode == BillRepeatModeDefOf.TargetCount)
                return $"{CurrentCount}/{targetCount}x";
            return "Something is broken :(";
        }
    }

    public BillRepeatModeDef RepeatMode { get; private set; } = BillRepeatModeDefOf.Forever;

    public BillStoreModeDef StoreMode { get; private set; } = BillStoreModeDefOf.BestStockpile;

    public Zone_Stockpile StoreZone { get; private set; }

    public CustomNetworkBill(NetworkBillStack stack)
    {
        _ownerStack = stack;
    }

    public CustomNetworkBill(float workAmount)
    {
        workAmountTotal = workAmountLeft = workAmount;
    }

    public void AssignToStack(NetworkBillStack owner)
    {
        _ownerStack = owner;
    }

    public void Destroy()
    {
        _ownerStack.Delete(this);
    }

    public CustomNetworkBill Clone()
    {
        var bill = new CustomNetworkBill(workAmountTotal);
        bill.repeatCount = repeatCount;
        bill.billName = billName + "_Copy";
        bill.RepeatMode = RepeatMode;

        bill._cost = new DefValueStack<NetworkValueDef, double>(_cost);
        bill._hasBeenPaid = _hasBeenPaid;

        bill.byProducts = new DefValueStack<NetworkValueDef, double>(byProducts);
        bill.results = new List<ThingDefCount>(results);
        return bill;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref billName, "billName");
        Scribe_Values.Look(ref repeatCount, "iterationsLeft");
        Scribe_Values.Look(ref workAmountTotal, "workAmountTotal");
        Scribe_Values.Look(ref workAmountLeft, "workAmountLeft");
        Scribe_Collections.Look(ref results, "results");

        ExposePayment();

        Scribe_Deep.Look(ref byProducts, "byProducts");
    }

    #region Job

    public bool ShouldDoNow()
    {
        if (!CanBePaid) return false;
        if (RepeatMode == BillRepeatModeDefOf.RepeatCount && repeatCount == 0) return false;
        if (RepeatMode == BillRepeatModeDefOf.TargetCount && CurrentCount >= targetCount) return false;
        return true;
    }

    public bool TryPayOrContinue()
    {
        if (HasBeenPaid) return true;
        if (TryPay()) return true;

        //Failed to pay...
        return false;
    }

    private void Reset()
    {
        workAmountLeft = workAmountTotal;
        ResetPayment();
    }

    public void Cancel()
    {
        TryRefund();
    }

    //Allocate network cost as "paid", refund if cancelled
    public void DoWork(Pawn pawn)
    {
        if (!TryPayOrContinue()) return;
        var num = pawn.GetStatValue(StatDefOf.GeneralLaborSpeed);
        var billBuilding = _ownerStack.ParentBuilding;
        if (billBuilding != null) num *= billBuilding.GetStatValue(StatDefOf.WorkSpeedGlobal);

        if (DebugSettings.fastCrafting) num *= 30f;
        workAmountLeft = Mathf.Clamp(workAmountLeft - num, 0, float.MaxValue);
    }

    public bool TryFinish(out List<Thing> products)
    {
        products = null;
        if (workAmountLeft > 0) return false;

        products = new List<Thing>();
        foreach (var defCount in results)
        {
            var desiredAmount = defCount.Count;
            while (desiredAmount > 0)
            {
                var possibleAmount = Mathf.Clamp(desiredAmount, 0, defCount.ThingDef.stackLimit);
                var thing = ThingMaker.MakeThing(defCount.ThingDef);
                products.Add(thing);

                thing.stackCount = possibleAmount;
                GenPlace.TryPlaceThing(thing, _ownerStack.ParentBuilding.InteractionCell, _ownerStack.ParentBuilding.Map,
                    ThingPlaceMode.Near);
                desiredAmount -= possibleAmount;
            }

            if (repeatCount > 0)
                repeatCount--;

            if (repeatCount is -1 or > 0)
                Reset();

            if (repeatCount == 0)
                _ownerStack.Delete(this);
        }

        if (!byProducts.IsEmpty)
            foreach (var byProduct in byProducts)
            {
                var network = _ownerStack.ParentComp[byProduct.Def.NetworkDef];
                if (network == null)
                    TLog.Warning(
                        $"Tried to add byproduct to non-existent network: {byProduct.Def.NetworkDef} | {byProduct.Def}");
                network?.Volume.TryAdd(byProduct.Def, byProduct.Value);
            }

        return true;
    }

    #endregion

    #region Storage Config (drop off location)

    public void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
    {
        StoreMode = mode;
        StoreZone = zone;
        if (StoreMode == BillStoreModeDefOf.SpecificStockpile != (StoreZone != null))
            Log.ErrorOnce("Inconsistent bill StoreMode data set", 75645354);
    }

    public bool CanPossiblyStoreInStockpile(Zone_Stockpile stockpile)
    {
        return stockpile.GetStoreSettings().AllowedToAccept(results[0].ThingDef);
    }

    #endregion
}