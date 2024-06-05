using Verse;

namespace TeleCore.RWExtensions.XML;

public class HierarchicalCategoryDef : Def
{
    public HierarchicalCategoryDef parent;
    public HierarchicalCategoryDef categoryKind;

    public override void PostLoad()
    {
        base.PostLoad();
        HierarchicalCategories.Register(this);
    }
}