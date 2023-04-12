﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace TeleCore
{
    public static class AIPatches
    {
        [HarmonyPatch(typeof(JobDriver)), HarmonyPatch(nameof(JobDriver.Cleanup))]
        public static class JobDriver_CleanupPatch
        {
            public static JobCondition _LastJobCondition;

            public static bool Prefix(JobCondition condition)
            {
                _LastJobCondition = condition;
                return true;
            }
        }
    }
}
