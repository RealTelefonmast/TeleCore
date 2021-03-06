using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class Window_Animator : Window
    {
        //
        internal AnimationWindowContainer content;

        public sealed override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        public override float Margin => 5f;

        public Window_Animator()
        {
            forcePause = true;
            doCloseX = false;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;

            layer = WindowLayer.Super;

            //
            content = new AnimationWindowContainer(this, new Rect(Vector2.zero, InitialSize), UIElementMode.Static);
        }

        public override void PreOpen()
        {
            base.PreOpen();
            content.Notify_Reopened();
        }

        public override void DoWindowContents(Rect inRect)
        {
            UIEventHandler.Begin();
            content.DrawElement(inRect);
            UIEventHandler.End();
        }
    }
}
