﻿using System.Collections.Generic;
using System.Linq;
using TeleCore.Network.Data;
using TeleCore.Network.Utility;
using TeleCore.Primitive;
using UnityEngine;
using Verse;

namespace TeleCore.Network.Bills;

public class NetworkBillStack : IExposable
{
    //
    public static readonly float MarketPriceFactor = 2.4f;
    public static readonly float WorkAmountFactor = 10;

    //Temp Custom Bill
    public int billID = 1;
    public string billName = "";
    private List<CustomNetworkBill> bills = new();

    //

    //Details
    private CustomNetworkBill detailsRequester;
    public Dictionary<CustomRecipeRatioDef, int> RequestedAmount = new();
    public string[] textBuffers;

    public NetworkBillStack(Comp_NetworkBillsCrafter parent)
    {
        ParentComp = parent;
        textBuffers = new string[Ratios.Count];
        foreach (var recipe in Ratios) RequestedAmount.Add(recipe, 0);

        ResetBillData();
    }

    public DefValueStack<NetworkValueDef, double> TotalCost { get; set; }
    public DefValueStack<NetworkValueDef,double> ByProducts { get; set; }

    public int TotalWorkAmount => TotalCost.IsEmpty ? 0 : TotalCost.Values.Sum(m => (int) (m.Value * WorkAmountFactor));

    //
    public Building ParentBuilding => ParentComp.parent;
    public Comp_NetworkBillsCrafter ParentComp { get; }

    public IEnumerable<INetworkPart> ParentNetParts => UsedNetworks?.Select(n => ParentComp[n]) ?? null;

    public IEnumerable<NetworkDef> UsedNetworks =>
        CurrentBill?.networkCost.Values.Select(t => t.Def.NetworkDef)?.Distinct() ?? null;

    public List<CustomRecipeRatioDef> Ratios => ParentComp.Props.UsedRatioDefs;

    public List<CustomNetworkBill> Bills => bills;
    public CustomNetworkBill CurrentBill => bills.FirstOrDefault(c => c?.ShouldDoNow() ?? false);
    public int Count => bills.Count;

    public CustomNetworkBill this[int index] => bills[index];

    public void ExposeData()
    {
        Scribe_Values.Look(ref billID, "billID");
        Scribe_Values.Look(ref billName, "billName");
        Scribe_Collections.Look(ref RequestedAmount, "requestAmount");
        Scribe_Collections.Look(ref bills, "bills", LookMode.Deep, this);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            for (var i = 0; i < RequestedAmount.Count; i++)
                textBuffers[i] = RequestedAmount.ElementAt(i).Value.ToString();
    }

    public void CreateBillFromDef(CustomRecipePresetDef presetDefDef)
    {
        var totalCost = presetDefDef.desiredResources.Sum(t => (int) (t.Value * WorkAmountFactor));
        var customBill = new CustomNetworkBill(totalCost);
        customBill.billName = presetDefDef.defName;
        customBill.networkCost = NetworkBillUtility.ConstructCustomCostStack(presetDefDef.desiredResources);
        if (presetDefDef.HasByProducts)
            customBill.byProducts = NetworkBillUtility.ConstructCustomCostStack(presetDefDef.desiredResources, true);
        customBill.billStack = this;
        customBill.results = presetDefDef.Results;
        bills.Add(customBill);
    }

    public void TryCreateNewBill()
    {
        if (TotalCost.IsEmpty) return;

        var customBill = new CustomNetworkBill(TotalWorkAmount);
        customBill.billName = billName;
        customBill.networkCost = new DefValueStack<NetworkValueDef, double>(TotalCost);

        if (!ByProducts.IsEmpty)
            customBill.byProducts = new DefValueStack<NetworkValueDef, double>(ByProducts);

        customBill.billStack = this;
        customBill.results = RequestedAmount.Where(m => m.Value > 0)
            .Select(m => new ThingDefCount(m.Key.result, m.Value)).ToList();
        bills.Add(customBill);
        billID++;

        //Clear Data
        ResetBillData();
    }

    public void PasteFromClipBoard(CustomNetworkBill clipBoardVal)
    {
        clipBoardVal.billStack = this;
        bills.Add(clipBoardVal);
    }

    public void Delete(CustomNetworkBill bill)
    {
        bill.Cancel();
        bills.Remove(bill);
    }

    private void ResetBillData()
    {
        billName = $"Custom Bill #{billID}";
        for (var i = 0; i < Ratios.Count(); i++)
        {
            textBuffers[i] = "0";
            RequestedAmount[Ratios[i]] = 0;
            TotalCost = new DefValueStack<NetworkValueDef, double>();
            ByProducts = new DefValueStack<NetworkValueDef, double>();
        }
    }

    //Drawing
    public void TryDrawBillDetails(Rect detailRect)
    {
        if (detailsRequester == null) return;
        Find.WindowStack.ImmediateWindow(GetHashCode(), detailRect, WindowLayer.Dialog, () =>
        {
            detailRect = detailRect.AtZero();
            TWidgets.DrawColoredBox(detailRect, TColor.BGDarker, TColor.WindowBGBorderColor, 1);
            CustomNetworkBillUtility.DrawDetails(detailRect.ContractedBy(5), detailsRequester);
        }, false, false, 0);
    }

    public void RequestDetails(CustomNetworkBill customNetworkBill)
    {
        if (detailsRequester == customNetworkBill)
        {
            detailsRequester = null;
            return;
        }

        detailsRequester = customNetworkBill;
    }
}