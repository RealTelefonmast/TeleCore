﻿using System.Linq;
using TeleCore.Network.Data;
using TeleCore.Network.Flow;

namespace TeleCore.Network.Bills;

public class NetworkCost
{
    public NetworkCostSet costSet;
    public bool useDirectStorage = false;

    public NetworkCostSet Cost => costSet;

    //Validation
    public bool CanPayWith(Comp_Network networkComp)
    {
        return networkComp.NetworkParts.Any(CanPayWith);
    }

    public bool CanPayWith(INetworkPart subPart)
    {
        return useDirectStorage ? CanPayWith(subPart.Volume) : CanPayWith(subPart.Network);
    }

    private bool CanPayWith(NetworkVolume fb)
    {
        if (fb.TotalValue < Cost.TotalCost) return false;
        var totalNeeded = Cost.TotalCost;

        if (Cost.HasSpecifics)
            foreach (var specificCost in Cost.SpecificCosts)
                if (fb.StoredValueOf(specificCost.valueDef) >= specificCost.value)
                    totalNeeded -= specificCost.value;

        if (Cost.mainCost > 0)
            foreach (var type in Cost.AcceptedValueTypes)
                totalNeeded -= fb.StoredValueOf(type);
        return totalNeeded == 0;
    }

    private bool CanPayWith(PipeNetwork wholeNetwork)
    {
        var totalNetworkValue = wholeNetwork.NetworkSystem.TotalValue;
        var totalNeeded = Cost.TotalCost;
        if (totalNetworkValue < totalNeeded) return false;
        //Check For Specifics
        if (Cost.HasSpecifics)
            foreach (var typeCost in Cost.SpecificCosts)
            {
                var specCost = typeCost.value;
                if (wholeNetwork.NetworkSystem.TotalValueFor(typeCost.valueDef) >= specCost)
                    totalNeeded -= specCost;
            }

        //Check For Generic Cost Value
        if (Cost.mainCost > 0)
            if (wholeNetwork.NetworkSystem.TotalValue >= Cost.mainCost)
                totalNeeded -= Cost.mainCost;

        return totalNeeded == 0;
    }

    //Process
    public void DoPayWith(Comp_Network network)
    {
        if (useDirectStorage)
            DoPayWithContainer(network);
        else
            DoPayWithNetwork(network);
    }

    private void DoPayWithContainer(Comp_Network structure)
    {
        var totalCost = Cost.TotalCost;
        if (totalCost <= 0) return;

        foreach (var typeCost in Cost.SpecificCosts)
        {
            var part = structure[typeCost.valueDef.NetworkDef];
            if (part.Network.NetworkSystem.TryConsume(part.Volume, typeCost.valueDef, typeCost.value))
                totalCost -= typeCost.value;
        }

        foreach (var type in Cost.AcceptedValueTypes)
        {
            var part = structure[type.NetworkDef];

            var result = part.Volume.TryRemove(type, totalCost);
            if (result) 
                totalCost -= result.Actual;
        }

        if (totalCost > 0)
            TLog.Warning($"Paying {this} with {structure.Thing} had leftOver {totalCost}");
        if (totalCost < 0)
            TLog.Warning($"Paying {this} with {structure.Thing} was too much: {totalCost}");
    }

    //TODO: Make totalcost a stack
    private void DoPayWithNetwork(Comp_Network structure)
    {
        var totalCost = Cost.TotalCost;
        if (totalCost <= 0) return;

        foreach (var storage in structure.NetworkParts.Select(s => s.Network)
                     .SelectMany(n => n.PartSet[NetworkRole.Storage]).TakeWhile(storage => !(totalCost <= 0)))
        {
            foreach (var typeCost in Cost.SpecificCosts)
                if (storage.Volume.TryConsume(typeCost.valueDef, typeCost.value))
                    totalCost -= typeCost.value;

            foreach (var type in Cost.AcceptedValueTypes)
            {
                var result = storage.Volume.TryRemove(type, totalCost);
                if (result) 
                    totalCost -= result.Actual;
            }
        }

        if (totalCost > 0)
            TLog.Warning($"Paying {this} with {structure.Thing} had leftOver: {totalCost}");
        if (totalCost < 0)
            TLog.Warning($"Paying {this} with {structure.Thing} was too much: {totalCost}");
    }

    public override string ToString()
    {
        return $"{Cost}|Direct: {useDirectStorage}";
    }
}