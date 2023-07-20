﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore;

public class SubMenuCategoryDef : Def
{
    public bool isDevCategory = false;
}

public class SubBuildMenu : Window, IExposable
{
    private static readonly Vector2 tabSize = new(118, 30);
    private static readonly Vector2 searchBarSize = new(125, 25);
    private static readonly float topBotMargin = 10f;
    private static readonly float sideMargin = 3f;
    private static readonly float iconSize = 30f;

    //
    private readonly Dictionary<SubMenuGroupDef, SubMenuCategoryDef> cachedSelection = new();
    private bool favoriteMenuActive;
    private List<BuildableDefScribed> favoriteOptions = new(); //Extra cache, Used to render only the favorited options
    private readonly HashSet<ThingDef> HighLightOptions = new();

    private readonly Dictionary<SubMenuGroupDef, Texture2D> iconByGroup = new();
    private BuildableDef inactiveDef;

    private Vector2 lastPos;

    //
    private SubBuildMenuDef menuDef;
    private Gizmo mouseOverGizmo;

    //SavedData
    private Dictionary<BuildableDefScribed, SubMenuOptionSettings> optionStates = new();
    private Vector2 scroller = Vector2.zero;
    private string searchText = "";

    public SubBuildMenu()
    {
        draggable = true;
        preventCameraMotion = false;
        doCloseX = true;

        windowRect.x = 5f;
        windowRect.y = 5f;
        doWindowBackground = false;
        doCloseButton = false;
        doCloseX = false;
    }

    public SubBuildMenu(SubBuildMenuDef menuDef)
    {
        this.menuDef = menuDef;

        //Window Settings
        draggable = true;
        preventCameraMotion = false;
        doCloseX = true;

        windowRect.x = 5f;
        windowRect.y = 5f;
        doWindowBackground = false;
        doCloseButton = false;
        doCloseX = false;

        Setup(menuDef);
    }

    private SubMenuGroupDef SelectedGroup { get; set; }
    private SubMenuCategoryDef SelectedCategoryDef => cachedSelection[SelectedGroup];
    private Designator CurrentDesignator => (Designator) (mouseOverGizmo ?? Find.DesignatorManager.SelectedDesignator);


    public DesignationTexturePack CurrentTexturePack => SelectedGroup.TexturePack ?? menuDef.TexturePack;

    //
    public override Vector2 InitialSize => new(400, 550);
    public override float Margin => 8;

    public void ExposeData()
    {
        //
        Scribe_Defs.Look(ref menuDef, nameof(menuDef));
        Scribe_Collections.Look(ref favoriteOptions, "favoriteOptions");
        Scribe_Collections.Look(ref optionStates, "optionStates", LookMode.Deep, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) Setup(menuDef);
    }

    private void Setup(SubBuildMenuDef menuDef)
    {
        //Menu Settings
        SelectedGroup = menuDef.subMenus.First();
        
        //
        lastPos = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);

        //Generate
        foreach (var def in menuDef.subMenus)
            //var path = (def.subPackPath ?? menuDef.superPackPath) ?? DefaultPackPath;
            //texturePacks.Add(def, new DesignationTexturePack(path));
            cachedSelection.Add(def, def.subCategories[0]);
    }

    public override bool OnCloseRequest()
    {
        if (MainButtonDefOf.Architect.TabWindow is MainTabWindow_Architect architect) architect.selectedDesPanel = null;
        return base.OnCloseRequest();
    }

    public override void DoWindowContents(Rect inRect)
    {
        //
        var searchBar = new Rect(new Vector2(inRect.xMax - searchBarSize.x, 0f), searchBarSize);
        var favoritesRect = new Rect(searchBar.x - (searchBarSize.y + 4), searchBar.y, searchBarSize.y, searchBarSize.y)
            .ContractedBy(2).Rounded();
        DoSearchBar(searchBar);

        //Favorited
        if (Widgets.ButtonImage(favoritesRect, TeleContent.Favorite_Filled)) favoriteMenuActive = !favoriteMenuActive;

        //SetupBG
        var menuRect = new Rect(0f, searchBarSize.y, windowRect.width, 526f);
        Widgets.DrawTextureRotated(menuRect, CurrentTexturePack.backGround, 0f);
        //Reduce Content Rect
        menuRect = new Rect(sideMargin, menuRect.y + topBotMargin, menuRect.width - sideMargin,
            menuRect.height - topBotMargin * 2);
        Widgets.BeginGroup(menuRect);
        {
            GroupSidebar(3);
            var extraDes = new Rect(2, menuRect.height - 75, iconSize, iconSize);
            DrawDesignator(extraDes, DesignatorUtility.FindAllowedDesignator<Designator_Deconstruct>());
            extraDes.y = extraDes.yMax + 5;
            DrawDesignator(extraDes, DesignatorUtility.FindAllowedDesignator<Designator_Cancel>());
            var DesignatorRect = new Rect(iconSize + sideMargin, 0f, menuRect.width - (iconSize + sideMargin),
                menuRect.height);
            Widgets.BeginGroup(DesignatorRect);
            {
                if (favoriteMenuActive)
                {
                    var size = new Vector2(80, 80);
                    var curXY = new Vector2(5f, 5f);

                    //
                    var favoriteTabLabelRect = new Rect(curXY, new Vector2(DesignatorRect.width, 32));
                    curXY.y += favoriteTabLabelRect.y;

                    //
                    Widgets.Label(favoriteTabLabelRect, "Favorites");
                    curXY.y += TWidgets.GapLine(curXY.x, curXY.y, DesignatorRect.width, 4, 0);

                    //
                    var main = new Rect(0f, curXY.y, DesignatorRect.width, DesignatorRect.height - curXY.y);
                    for (var i = favoriteOptions.Count - 1; i >= 0; i--)
                    {
                        var def = (BuildableDef) favoriteOptions[i];
                        if (!DebugSettings.godMode &&
                            def.HasSubMenuExtension(out var subMenu) && subMenu.isDevOption) continue;
                        if (SubMenuThingDefList.IsActive(menuDef, def))
                            Designator(def, main, size, ref curXY);
                        else
                            InactiveDesignator(def, main, size, ref curXY);
                    }
                }
                else
                {
                    var subCats = SelectedGroup.subCategories;
                    var curXY = Vector2.zero;
                    foreach (var cat in subCats)
                    {
                        if (cat.isDevCategory && !DebugSettings.godMode) continue;

                        var tabRect = new Rect(curXY, tabSize);
                        var clickRect = new Rect(tabRect.x + 5, tabRect.y, tabRect.width - 10, tabRect.height);
                        var tex = cat == SelectedCategoryDef || Mouse.IsOver(clickRect)
                            ? CurrentTexturePack.tabSelected
                            : CurrentTexturePack.tab;
                        Widgets.DrawTextureFitted(tabRect, tex, 1f);
                        if (HasUnDiscovered(menuDef, SelectedGroup, cat))
                            TWidgets.DrawTextureInCorner(tabRect, TeleContent.Undiscovered, 7,
                                TextAnchor.UpperRight, new Vector2(-6, 3));
                        //DrawUndiscovered(tabRect, new Vector2(-6, 3));
                        //Widgets.DrawTextureFitted(tabRect, TiberiumContent.Tab_Undisc, 1f);
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Text.Font = GameFont.Small;
                        string catLabel = cat.LabelCap;
                        if (Text.CalcSize(catLabel).y > tabRect.width) Text.Font = GameFont.Tiny;

                        Widgets.Label(tabRect, catLabel);
                        Text.Font = GameFont.Tiny;
                        Text.Anchor = 0;

                        AdjustXY(ref curXY, tabSize.x - 10f, tabSize.y, tabSize.x * 3);
                        if (Widgets.ButtonInvisible(clickRect))
                        {
                            searchText = "";
                            SetSelectedCat(cat);
                        }
                    }

                    //
                    XYEndCheck(ref curXY, tabSize.y, tabSize.x * 3, subCats.Count);
                    DrawSubThingGroup(new Rect(0f, curXY.y, DesignatorRect.width, DesignatorRect.height - curXY.y),
                        SelectedGroup, SelectedCategoryDef);
                }
            }
            Widgets.EndGroup();
        }
        Widgets.EndGroup();
    }

    private void DrawSubThingGroup(Rect main, SubMenuGroupDef groupDef, SubMenuCategoryDef categoryDef)
    {
        if (groupDef != null && categoryDef != null)
        {
            Widgets.BeginGroup(main);
            {
                var size = new Vector2(80, 80);
                var curXY = new Vector2(5f, 5f);
                var things = searchText.NullOrEmpty()
                    ? SubMenuThingDefList.Categorized[groupDef][categoryDef]
                    : ItemsBySearch(searchText);
                var viewRect = new Rect(0f, 0f, main.width,
                    10 + (float) (Math.Round((decimal) (things.Count / 4), 0, MidpointRounding.AwayFromZero) + 1) *
                    size.x);
                var scrollerRect = new Rect(0f, 0f, main.width, main.height + 5);
                Widgets.BeginScrollView(scrollerRect, ref scroller, viewRect, false);
                {
                    mouseOverGizmo = null;
                    inactiveDef = null;
                    foreach (var def in things)
                    {
                        if (!DebugSettings.godMode && def.HasSubMenuExtension(out var subMenu) &&
                            subMenu.isDevOption) continue;
                        if (SubMenuThingDefList.IsActive(menuDef, def))
                            Designator(def, main, size, ref curXY);
                        else
                            InactiveDesignator(def, main, size, ref curXY);
                    }
                }
                Widgets.EndScrollView();
            }
            Widgets.EndGroup();
        }
    }

    private List<BuildableDef> ItemsBySearch(string searchText)
    {
        return SubMenuThingDefList.Categorized[SelectedGroup].SelectMany(cat => cat.Value).Where(d =>
            SubMenuThingDefList.IsActive(menuDef, d) && d.label.ToLower().Contains(searchText.ToLower())).ToList();
    }

    private void Designator(BuildableDef def, Rect main, Vector2 size, ref Vector2 XY)
    {
        var rect = new Rect(new Vector2(XY.x, XY.y), size);
        GUI.color = new Color(1, 1, 1, 0.80f);
        var mouseOver = Mouse.IsOver(rect);
        var tex = mouseOver ? CurrentTexturePack.designatorSelected : CurrentTexturePack.designator;
        Widgets.DrawTextureFitted(rect, tex, 1f);
        GUI.color = mouseOver ? new Color(1, 1, 1, 0.45f) : Color.white;
        var icon = def.uiIcon != null ? def.uiIcon : BaseContent.BadTex;
        var texCoords = new Rect(0f, 0f, 1f, 1f);
        texCoords = def is TerrainDef ? Widgets.CroppedTerrainTextureRect(icon) : texCoords;
        Widgets.DrawTextureFitted(rect.ContractedBy(2), icon, 1, Vector2.one, texCoords);
        GUI.color = Color.white;
        if (def.HasSubMenuExtension(out var subMenu) && subMenu.isDevOption)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = TColor.White075;
            Widgets.Label(rect, "DEV");
            GUI.color = Color.white;
            Text.Anchor = default;
            Text.Font = GameFont.Small;
        }

        if (HighLightOptions.Contains(def)) Widgets.DrawTextureFitted(rect, TeleContent.Undiscovered, 1);

        var optionDiscovered = OptionIsDiscovered(def);
        if (!optionDiscovered)
            TWidgets.DrawTextureInCorner(rect, TeleContent.Undiscovered, 7, TextAnchor.UpperRight, new Vector2(-5, 5));
        //DrawUndiscovered(rect, new Vector2(-5, 5));
        //Widgets.DrawTextureFitted(rect, TiberiumContent.Des_Undisc, 1f);
        var favorited = OptionIsFavorited(def);
        var favTex = favorited ? TeleContent.Favorite_Filled : TeleContent.Favorite_Unfilled;
        TWidgets.DrawTextureInCorner(rect, favTex, 16, TextAnchor.UpperLeft, new Vector2(5, 5), () =>
        {
            if (ToggleOptionFavorite(def))
                favoriteOptions.Add(def);
            else
                favoriteOptions.Remove(def);
        });

        if (mouseOver)
        {
            if (!optionDiscovered) SetOptionDiscovered(def);

            var extension = def.SubMenuExtension();
            var thisOrGroupDevOption = extension.isDevOption || extension.groupDef.isDevGroup ||
                                       extension.category.isDevCategory;
            mouseOverGizmo = thisOrGroupDevOption
                ? GenData.GetDesignatorFor<Designator_BuildGodMode>(def)
                : GenData.GetDesignatorFor<Designator_Build>(def);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, def.LabelCap);
            Text.Anchor = 0;
            TooltipHandler.TipRegion(rect, def.LabelCap);
        }

        if (Widgets.ButtonInvisible(rect))
        {
            mouseOverGizmo.ProcessInput(null);
            Event.current.Use();
        }

        AdjustXY(ref XY, size.x, size.x, main.width, 5);
    }

    private void SetSelectedCat(SubMenuCategoryDef def)
    {
        cachedSelection[SelectedGroup] = def;
    }

    private void DoSearchBar(Rect textArea)
    {
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;

        if (searchText.NullOrEmpty())
        {
            GUI.color = new Color(1, 1, 1, 0.75f);
            Widgets.Label(textArea.ContractedBy(2), "Search..");
            GUI.color = Color.white;
        }

        searchText = Widgets.TextArea(textArea, searchText);
        Text.Anchor = 0;
    }

    private void GroupSidebar(float yPos)
    {
        var list = menuDef.subMenus;
        for (var i = 0; i < list.Count; i++)
        {
            var groupDef = list[i];
            if (groupDef.isDevGroup && !DebugSettings.godMode) continue;

            var partRect = new Rect(0f, yPos + (iconSize + 6) * i, iconSize, iconSize);
            var sel = Mouse.IsOver(partRect) || SelectedGroup == groupDef;
            GUI.color = sel ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawTextureFitted(partRect, IconForGroup(groupDef), 1f);
            GUI.color = Color.white;
            if (HasUnDiscovered(menuDef, groupDef))
                TWidgets.DrawTextureInCorner(partRect, TeleContent.Undiscovered, 8, TextAnchor.UpperRight);
            //DrawUndiscovered(partRect);
            if (Widgets.ButtonInvisible(partRect))
            {
                searchText = "";
                SelectedGroup = groupDef;
            }
        }
    }

    private void InactiveDesignator(BuildableDef def, Rect main, Vector2 size, ref Vector2 XY)
    {
        var rect = new Rect(new Vector2(XY.x, XY.y), size);
        GUI.color = Color.grey;
        var mouseOver = Mouse.IsOver(rect);
        var tex = mouseOver ? CurrentTexturePack.designatorSelected : CurrentTexturePack.designator;
        Widgets.DrawTextureFitted(rect, tex, 1f);
        Widgets.DrawTextureFitted(rect.ContractedBy(2), def.uiIcon, 1);
        GUI.color = Color.white;
        if (Mouse.IsOver(rect))
            inactiveDef = def;

        AdjustXY(ref XY, size.x, size.x, main.width, 5);
    }

    //
    private void DrawDesignator(Rect rect, Designator designator)
    {
        if (Widgets.ButtonImage(rect, designator.icon as Texture2D)) designator.ProcessInput(null);
    }

    private void AdjustXY(ref Vector2 XY, float xIncrement, float yIncrement, float maxWidth, float minX = 0f)
    {
        if (XY.x + xIncrement * 2 > maxWidth)
        {
            XY.x = minX;
            XY.y += yIncrement;
        }
        else
        {
            XY.x += xIncrement;
        }
    }

    private void XYEndCheck(ref Vector2 XY, float yIncrement, float maxWidth, int itemCount)
    {
        //
        if (XY.x != 0) XY.y += yIncrement;
    }

    private Texture2D IconForGroup(SubMenuGroupDef group)
    {
        if (iconByGroup.TryGetValue(group, out var tex)) return tex;

        if (group.groupIconPath == null)
            tex = BaseContent.BadTex;

        tex ??= ContentFinder<Texture2D>.Get(group.groupIconPath, false) ?? BaseContent.BadTex;
        iconByGroup.Add(group, tex);
        return tex;
    }

    public struct SubMenuOptionSettings : IExposable
    {
        public bool _isFavorited;
        public bool _isDiscovered;

        public SubMenuOptionSettings(bool isFavorited, bool isDiscovered)
        {
            _isFavorited = isFavorited;
            _isDiscovered = isDiscovered;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _isDiscovered, "isDiscovered");
            Scribe_Values.Look(ref _isFavorited, "isFavorite");
        }
    }

    #region Menu Item States

    public bool OptionIsFavorited(BuildableDef def)
    {
        return optionStates.TryGetValue(def, out var value) && value._isFavorited;
    }

    public bool OptionIsDiscovered(BuildableDef def)
    {
        return optionStates.TryGetValue(def, out var value) && value._isDiscovered;
    }

    public bool ToggleOptionFavorite(BuildableDef def)
    {
        if (optionStates.TryGetValue(def, out var state))
        {
            state._isFavorited = !state._isFavorited;
            optionStates[def] = state;
            return state._isFavorited;
        }

        optionStates.Add(def, new SubMenuOptionSettings(true, false));
        return true;
    }

    public void SetOptionDiscovered(BuildableDef def)
    {
        if (OptionIsDiscovered(def)) return;
        if (optionStates.TryGetValue(def, out var state))
        {
            state._isDiscovered = true;
            optionStates[def] = state;
            return;
        }

        optionStates.Add(def, new SubMenuOptionSettings(false, true));
    }

    #endregion

    #region Discovery

    public bool HasUnDiscovered(SubBuildMenuDef inMenu, SubMenuGroupDef group)
    {
        return SubMenuThingDefList.Categorized[group].Any(d => HasUnDiscovered(inMenu, group, d.Key));
    }

    public bool HasUnDiscovered(SubBuildMenuDef inMenu, SubMenuGroupDef group, SubMenuCategoryDef categoryDef)
    {
        return SubMenuThingDefList.Categorized[group][categoryDef]
            .Any(d => !OptionIsDiscovered(d) && SubMenuThingDefList.IsActive(inMenu, d));
    }

    #endregion

    #region Menu Toggle

    public static void ToggleOpen(SubBuildMenuDef subMenuDef, bool opening)
    {
        if (!StaticData.windowsByDef.TryGetValue(subMenuDef, out var window))
        {
            window = new SubBuildMenu(subMenuDef);
            StaticData.windowsByDef.Add(subMenuDef, window);
        }

        if (window.IsOpen && !opening)
        {
            window.lastPos = window.windowRect.center; // GUI.wind.center; //window.;
            window.Close();
        }
        else
        {
            Find.WindowStack.Add(window);
            window.windowRect.center = window.lastPos;
        }
    }

    public static void ResetMenuWindow(SubBuildMenuDef subMenuDef)
    {
        TLog.Message($"Resetting: {subMenuDef}");
        if (StaticData.windowsByDef.TryGetValue(subMenuDef, out var window))
            window.windowRect.center = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);
    }

    #endregion
}