using UnityEngine;
using Verse;

namespace TeleCore.Animations;

public class DevTool_Animation : Window
{
    private AnimationUIContainer _container;

    public DevTool_Animation()
    {
        _container.Begin();
    }
    
    public override void PreOpen()
    {
        base.PreOpen();
        _container.Notify_Reopened();
    }

    public override void DoWindowContents(Rect inRect)
    {
        _container.Draw(inRect);
        UIElementS.Begin().Draw(rect =>
        {
            
        });
    }
}