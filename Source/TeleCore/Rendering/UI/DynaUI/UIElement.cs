﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TeleCore.Rendering.UI.DynaUI.Events;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public enum UIElementMode
    {
        Dynamic,
        Static,
        Fill
    }
    public enum UIElementState
    {
        Open,
        Collapsed, 
        Closed
    }

    public abstract class UIElement : IDraggable, IFocusable
    {
        //Relation Holding
        protected UIElement _parent;
        protected readonly List<UIElement> _children = new();
        
        //Local Data
        protected Color bgColor = TColor.MenuSectionBGFillColor;
        protected Color borderColor = TColor.MenuSectionBGBorderColor;
        protected string label, title;
        protected bool hasTopBar = true;
        
        //TODO: Add Style: protected UIElementStyle style;
        //Transform
        private Vector2 position = Vector2.zero;
        private Vector2 size;
        private float rotation;

        //
        private Rect? overrideRect;
        
        //Internal Dragging
        private Vector2? startDragPos, endDragPos, oldPos;

        public Vector2 StartDragPos => startDragPos ?? Vector2.zero;
        public Vector2 EndDragPos => endDragPos ?? Vector2.zero;
        public Vector2 CurrentDragDiff => endDragPos.HasValue ? endDragPos.Value - startDragPos.Value : Vector2.zero;
        protected Vector2 CurrentDragResult => oldPos.HasValue ? new Vector2(oldPos.Value.x + CurrentDragDiff.x, oldPos.Value.y + CurrentDragDiff.y) : position;

        //Events
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event ElementSelectedEventHandler ElementSelected;

        //Properties
        public UIElementMode UIMode { get; private set; } = UIElementMode.Dynamic;
        public UIElementState UIState { get; protected set; } = UIElementState.Open;

        public virtual int Priority => 999;
        public int RenderLayer { get; set; } = 0;
        public bool CanAcceptAnything => IsActive && UIState == UIElementState.Open;

        protected bool IsFocused => UIEventHandler.IsFocused(this);
        protected bool IsLocked { get; private set; }
        protected bool ClickedIntoTop { get; private set; }

        public virtual bool CanBeFocused => true;
        public virtual bool CanDoRightClickMenu => true;
        public bool IsActive { get; set; } = true;

        public object DragAndDropData { get; protected set; }

        //Relations
        public UIElement Parent
        {
            get => _parent;
            set => _parent = value;
        }

        public virtual List<UIElement> ChildElements => _children;

        public virtual UIContainerMode ContainerMode => UIContainerMode.InOrder;

        //Main 
        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                overrideRect = new Rect(position.x, position.y, size.x, size.y);
                Notify_StateChanged();
            }
        }

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                overrideRect = new Rect(position.x, position.y, size.x, size.y);
                Notify_StateChanged();
            }
        }

        public Rect Rect
        {
            get => (overrideRect ?? new Rect(Position, Size)).Rounded();
            private set
            {
                overrideRect = value;
                position = new Vector2(value.x, value.y);
                size = new Vector2(value.width, value.height);
                Notify_StateChanged();
            }
        }

        public Rect InRect => new Rect(Rect.x, hasTopBar ? TopRect.yMax : Rect.y, Rect.width, Rect.height - (hasTopBar ? TopRect.height : 0));

        public Rect? DragContext => _parent?.Rect ?? null;

        public string Title
        {
            get => title;
            protected set => title = value;
        }

        public virtual string Label => "New Element";

        //Input Rect Data
        protected Rect TopRect => new Rect(position.x, position.y, size.x, UIConsts.BorderMargin);
        protected virtual Rect DragAreaRect => TopRect;
        public virtual Rect FocusRect => Rect;

        protected bool CanMove => UIMode == UIElementMode.Dynamic && ClickedIntoTop && !IsLocked && IsInDragArea();

        //Constructors
        protected UIElement(UIElementMode mode)
        {
            this.UIMode = mode;
            CollectionChanged += NotifyCollectionChanged;
            ElementSelected += NotifyElementSelected;
        }

        protected UIElement(Rect rect, UIElementMode mode)
        {
            this.Rect = rect;
            this.UIMode = mode;
            CollectionChanged += NotifyCollectionChanged;
            ElementSelected += NotifyElementSelected;
        }

        protected UIElement(Vector2 pos, Vector2 size, UIElementMode mode)
        {
            this.size = size;
            Position = pos;
            this.UIMode = mode;
            CollectionChanged += NotifyCollectionChanged;
            ElementSelected += NotifyElementSelected;
        }

        private void SetParent(UIElement parent) => this._parent = parent;
        private void SetPosition(Vector2 pos) => this.Position = pos;
        private void SetSize(Vector2 size) => this.Size = size;

        public void SetProperties(UIElement parent = null, Vector2? pos = null, Vector2? size = null)
        {
            if (parent != null)
                SetParent(parent);
            if (pos.HasValue)
                SetPosition(pos.Value);
            if (size.HasValue)
                SetSize(size.Value);
        }

        //Visibility
        public void ToggleOpen()
        {
            //if (parent is not ToolBar) return;
            UIState = UIState == UIElementState.Open ? UIElementState.Closed : UIElementState.Open;
        }

        public void SetVisibility(UIElementState state)
        {
            UIState = state;
        }

        //Relation Functions
        public T GetChildElement<T>() where T : UIElement
        {
            return (T)ChildElements.First(t => t is T);
        }

        //Relation Changes
        public void AddElement(UIElement newElement, Vector2? pos = null)
        {
            switch (ContainerMode)
            {
                case UIContainerMode.InOrder:
                    ChildElements.Add(newElement);
                    break;
                case UIContainerMode.Reverse:
                    ChildElements.Insert(0, newElement);
                    break;
            }
            newElement.SetParent(this);
            if(pos.HasValue)
                newElement.SetPosition(pos.Value);

            //
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newElement));
            
            //Notify_AddedElement(newElement);
            //TODO: Add More Events
            newElement.Notify_AddedToParent(this);
        }

        public void RemoveElement(UIElement element)
        {
            ChildElements.Remove(element);
            element.SetParent(null);
            
            //
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element));
            
            //TODO: Add More Events
            element.Notify_RemovedFromParent(this);
        }

        //protected virtual void Notify_AddedElement(UIElement newElement) { }
        //protected virtual void Notify_RemovedElement(UIElement newElement) { }

        protected virtual void Notify_AddedToParent(UIElement parent) { }
        protected virtual void Notify_RemovedFromParent(UIElement parent){}

        //Relation State Change
        private void Notify_StateChanged()
        {
            _parent?.Notify_ChildElementChanged(this);
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }
        
        private void OnElementSelected(ElementSelectedEventArgs e)
        {
            ElementSelected(this, e);
        }

        protected virtual void NotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        
        protected virtual void NotifyElementSelected(object sender, ElementSelectedEventArgs e) { }
        
        protected virtual void Notify_ChildElementChanged(UIElement element)
        {
        }

        public virtual void Notify_ElementSelected(UIElement element, int index)
        {
        }

        //
        protected virtual bool IsInDragArea()
        {
            return Mouse.IsOver(DragAreaRect);
        }

        //Drawing
        public void DrawElement(Rect? overrideRect = null)
        {
            if (UIState == UIElementState.Closed) return;

            UIEventHandler.RegisterLayer(this);
            if (overrideRect.HasValue)
                Rect = overrideRect.Value;

            //
            HandleEvent();

            //
            TWidgets.DrawColoredBox(Rect, bgColor, borderColor, 1);
            TWidgets.DrawColoredBox(InRect, bgColor, borderColor, 1);
            if(hasTopBar) DrawTopBar();

            //
            if (UIState == UIElementState.Collapsed) return;

            //
            var drawnInRect = InRect.ContractedBy(1);

            //Custom Drawing
            DragAndDropData = null;
            DrawContentsBeforeRelations(drawnInRect);
            DrawChildren(drawnInRect);
            DrawContentsAfterRelations(drawnInRect);
        }

        //Draw Relations
        protected void DrawChildren(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            switch (ContainerMode)
            {
                case UIContainerMode.InOrder:
                {
                    for (var i = 0; i < ChildElements.Count; i++)
                    {
                        var element = ChildElements[i];
                        element.DrawElement();
                    }

                    break;
                }
                case UIContainerMode.Reverse:
                {
                    for (int i = ChildElements.Count - 1; i >= 0; i--)
                    {
                        var element = ChildElements[i];
                        element.DrawElement();
                    }
                    break;
                }
            }
            Widgets.EndGroup();
        }

        private void DrawTopBar()
        {
            TWidgets.DrawBoxHighlightIfMouseOver(DragAreaRect);

            Widgets.BeginGroup(TopRect);
            {
                WidgetRow topBarRow = new WidgetRow();
                topBarRow.Init(TopRect.x, TopRect.y, UIDirection.RightThenDown);
                DrawSettings(topBarRow);
                if (title != null)
                {
                    topBarRow.Label(Title);
                }
                DoTopBarExtra(topBarRow);
                DrawTopBarExtras(TopRect.AtZero());
            }
            Widgets.EndGroup();
        }

        private void DrawSettings(WidgetRow row)
        {
            if (UIMode == UIElementMode.Dynamic)
            {
                if (row.ButtonIcon(IsLocked ? TeleContent.LockClosed : TeleContent.LockOpen))
                {
                    IsLocked = !IsLocked;
                }
            }
            /*
            if (row.ButtonIcon(TeleContent.DeleteX))
            {
                ToggleOpen();
            }
            */
        }

        protected virtual void DoTopBarExtra(WidgetRow row) { }

        //Event Handling
        private void HandleEvent()
        {
            Event curEvent = Event.current;
            Vector2 mousePos = curEvent.mousePosition;

            bool isInContext = Rect.Contains(mousePos);
            //if (!isInContext) return;

            if (curEvent.type == EventType.MouseDown && isInContext)
            {
                startDragPos ??= mousePos;
                oldPos ??= position;
                UIEventHandler.StartFocus(this);

                if (Mouse.IsOver(TopRect))
                {
                    ClickedIntoTop = true;
                }

                //FloatMenu
                if (curEvent.button == 1 && Mouse.IsOver(FocusRect) && CanDoRightClickMenu)
                {
                    var options = RightClickOptions()?.ToList();
                    if (options != null && options.Any())
                    {
                        FloatMenu menu = new FloatMenu(options);
                        menu.vanishIfMouseDistant = true;
                        Find.WindowStack.Add(menu);
                    }
                }
            }

            if (curEvent.type == EventType.MouseDrag && startDragPos != null)
            {
                endDragPos = mousePos;
            }

            //Custom Handling
            HandleEvent_Custom(curEvent, isInContext);

            //Handle Pos Manipulation
            if (IsFocused)
            {
                if (UIDragger.IsBeingDragged(this) || (curEvent.type == EventType.MouseDrag && CanMove))
                    UIDragger.Notify_ActiveDrag(this, curEvent);

                if (UIDragNDropper.IsSource(this) || DragAndDropData != null && curEvent.type == EventType.MouseDrag)
                    UIDragNDropper.Notify_DraggingData(this, DragAndDropData, curEvent);
            }

            //Reset
            if (curEvent.type == EventType.MouseUp)
            {
                startDragPos = oldPos = endDragPos = null;
                ClickedIntoTop = false;
                UIEventHandler.StopFocus(this);
            }
        }

        //Extensions
        protected virtual IEnumerable<FloatMenuOption> RightClickOptions()
        {
            return null;
        }

        //Extensions
        protected virtual void HandleEvent_Custom(Event ev, bool inContext = false)
        {
        }

        protected virtual void DrawContentsBeforeRelations(Rect inRect)
        {
        }

        protected virtual void DrawContentsAfterRelations(Rect inRect)
        {
        }

        protected virtual void DrawTopBarExtras(Rect topRect)
        {

        }
    }
}
