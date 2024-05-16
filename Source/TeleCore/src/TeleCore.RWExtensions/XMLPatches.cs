﻿using System.Xml;
using HarmonyLib;
using TeleCore.Loader.XML;
using Verse;

namespace TeleCore.Loader;

internal static class XMLPatches
{
    [HarmonyPatch(typeof(DirectXmlLoader), nameof(DirectXmlLoader.DefFromNode))]
    static class DefFromNodePatch
    {
        // ReSharper disable once InconsistentNaming
        static void Postfix(XmlNode node, Def? __result)
        {
            if(__result == null) return;
            var attribute = node.Attributes?["Tags"];
            if (attribute == null) return;
            var tags = attribute.Value.Split(',');
            TaggedDefs.RegisterDefTags(__result, tags);
        }
    }
}