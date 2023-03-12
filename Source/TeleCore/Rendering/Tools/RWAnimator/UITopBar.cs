﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class TopBarButtonOption
    {
        private string label;
        private Action action;

        public float OptionHeight => Text.CalcSize(label).y;

        public TopBarButtonOption(string label, Action action)
        {
            this.label = label;
            this.action = action;
        }

        public bool DoGUI(Rect rect)
        {
            TWidgets.DrawSelectionHighlight(rect);
            Widgets.Label(rect, label);
            if (Widgets.ButtonInvisible(rect))
            {
                action.Invoke();
                return true;
            }
            return false;
        }
    }

    public class TopBarButtonMenu
    {
        private const int menuWidth = 150;
        private const int optionMargin = 2;

        //
        private string label;
        private List<TopBarButtonOption> optionList;
        private Action action;

        //
        private bool isOpen = false;

        private Color baseColor = Color.white;

        public bool IsOpen => isOpen;
        public float TotalHeight => optionList?.Sum(o => o.OptionHeight + (2 * optionMargin)) ?? 0;
        public float ButtonWidth => Text.CalcSize(label).x + 10;

        public TopBarButtonMenu(string menuLabel, List<TopBarButtonOption> optionList)
        {
            this.label = menuLabel;
            this.optionList = optionList;
        }

        public TopBarButtonMenu(string menuLabel, Action action)
        {
            this.label = menuLabel;
            this.action = action;
        }

        public void Close()
        {
            isOpen = false;
        }

        public void Open()
        {
            isOpen = true;
        }

        private void DoMenu(Rect rect)
        {
            Rect menuRect = new Rect(rect.x, rect.yMax, menuWidth, TotalHeight);

            TWidgets.DrawColoredBox(menuRect, TColor.BlueHueBG, TColor.MenuSectionBGBorderColor, 1);
            Widgets.BeginGroup(menuRect);
            {
                try
                {
                    var curY = 0f;
                    foreach (var barButtonOption in optionList)
                    {
                        var height = barButtonOption.OptionHeight + (2 * optionMargin);
                        var optionRect = new Rect(0, curY, menuWidth, height).ContractedBy(optionMargin);
                        if (barButtonOption.DoGUI(optionRect))
                            Close();

                        curY += height;
                    }
                }
                catch (Exception ex)
                {
                    TLog.Error($"Some crap is happening in UI: {ex}");
                }
            }
            Widgets.EndGroup();
        }

        public void DoButton(Rect rect)
        {
            TWidgets.DrawSelectionHighlight(rect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = default;

            if (Widgets.ButtonInvisible(rect))
            {
                if (action != null)
                {
                    action.Invoke();
                    return;
                }
                Open();
            }

            if (isOpen)
            {
                //Do Menu
                DoMenu(rect);

                //Disappear on mouse distant
                Rect r = new Rect(0f, 0f, menuWidth, this.TotalHeight).ContractedBy(-5f);
                if (!r.Contains(Event.current.mousePosition))
                {
                    float num = GenUI.DistFromRect(r, Event.current.mousePosition);
                    this.baseColor = new Color(1f, 1f, 1f, 1f - num / 95f);
                    if (num > 95f)
                    {
                        this.Close();
                        return;
                    }
                }
            }
        }
    }

    public class UITopBar : UIElement
    {
        //
        private List<TopBarButtonMenu> buttons;
        //
        private Action closeAction;
        private bool doCloseButton = false;

        private TopBarButtonMenu CurrentOpen => buttons.FirstOrFallback(b => b.IsOpen);
        private bool AnyMenuOpen => CurrentOpen != null;

        public UITopBar(List<TopBarButtonMenu> buttons) : base(UIElementMode.Static)
        {
            this.buttons = buttons;
            hasTopBar = false;

            this.bgColor = TColor.BGDarker;
            this.borderColor = Color.clear;
        }

        protected override void DrawContentsBeforeRelations(Rect inRect)
        {
            var curX = inRect.x;
            foreach (var button in buttons)
            {
                var buttonRect = new Rect(curX, inRect.y, button.ButtonWidth, inRect.height);
                button.DoButton(buttonRect);

                if (AnyMenuOpen && Mouse.IsOver(buttonRect) && button != CurrentOpen)
                {
                    CurrentOpen.Close();
                    button.Open();
                }

                curX += button.ButtonWidth;
            }

            if (doCloseButton)
            {
                if (TWidgets.CloseButtonCustom(inRect, inRect.height))
                {
                    closeAction.Invoke();
                }
            }
        }

        public void AddCloseButton(Action action)
        {
            doCloseButton = true;
            closeAction = action;
        }
    }
}
