﻿using System.Collections.Generic;
using System.Text;
using Verse;

namespace TeleCore.FlowCore.Utility;

internal static class NetworkBillUtility
{
    public static DefValueStack<Flow.Values.NetworkValueDef, double> ConstructCustomCostStack(List<DefValueLoadable<CustomRecipeRatioDef, int>> list, bool isByProduct = false)
    {
        var stack = new DefValueStack<Flow.Values.NetworkValueDef, double>();
        foreach (var defIntRef in list)
        {
            if (isByProduct)
            {
                foreach (var ratio in defIntRef.Def.byProducts)
                    stack += new DefValue<Flow.Values.NetworkValueDef, double>(ratio.Def, ratio.Value * defIntRef.Value);
                continue;
            }

            foreach (var ratio in defIntRef.Def.inputRatio)
                stack += new DefValue<Flow.Values.NetworkValueDef, double>(ratio.Def, ratio.Value * defIntRef.Value);
        }

        return stack;
    }

    public static DefValueStack<Flow.Values.NetworkValueDef, double> ConstructCustomCostStack(
        IDictionary<CustomRecipeRatioDef, int> requestedAmount, bool isByProduct = false)
    {
        var stack = new DefValueStack<Flow.Values.NetworkValueDef, double>();
        foreach (var defIntRef in requestedAmount)
        {
            if (isByProduct)
            {
                foreach (var ratio in defIntRef.Key.byProducts)
                    stack += new DefValue<Flow.Values.NetworkValueDef, double>(ratio.Def, ratio.Value * defIntRef.Value);
                continue;
            }

            foreach (var ratio in defIntRef.Key.inputRatio)
                stack += new DefValue<Flow.Values.NetworkValueDef, double>(ratio.Def, ratio.Value * defIntRef.Value);
        }

        return stack;
    }

    public static string CostLabel(DefValueStack<Flow.Values.NetworkValueDef, double> values)
    {
        if (values.IsEmpty) return "N/A";
        var sb = new StringBuilder();
        sb.Append("(");
        for (var i = 0; i < values.Length; i++)
        {
            var input = values[i];
            sb.Append($"{input.Value}{input.Def.labelShort.Colorize(input.Def.valueColor)}");
            if (i + 1 < values.Length)
                sb.Append(" ");
        }

        sb.Append(")");
        return sb.ToString();
    }

    public static string CostLabel(List<DefValueLoadable<Flow.Values.NetworkValueDef, float>> values)
    {
        if (values.NullOrEmpty()) return "N/A";
        var sb = new StringBuilder();
        sb.Append("(");
        for (var i = 0; i < values.Count; i++)
        {
            var input = values[i];
            sb.Append($"{input.Value}{input.Def.labelShort.Colorize(input.Def.valueColor)}");
            if (i + 1 < values.Count)
                sb.Append(" ");
        }

        sb.Append(")");
        return sb.ToString();
    }
}