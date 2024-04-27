﻿using System.Collections.Generic;
using Verse;

namespace TeleCore;

public class SubMenuExtension : DefModExtension
{
    public SubMenuCategoryDef category;
    public SubMenuGroupDef groupDef;
    public bool isDevOption;

    public SubBuildMenuDef ParentDef => groupDef.parentDef;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors()) yield return error;
        if (!groupDef.subCategories.Contains(category))
            yield return
                $"SubMenuGroupDef '{groupDef.defName}' does not contain any SubMenuCategoryDef '{category.defName}'";
    }
}