﻿using System;
using System.IO;
using UnityEngine;
using Verse;

namespace TeleCore;

public class Dialog_DirectoryBrowser : Window
{
    private DirectoryInfo currentDir;
    private DirectoryInfo rootLimit;
    private readonly Action<DirectoryInfo> selAction;
    private readonly string titleString;

    public Dialog_DirectoryBrowser()
    {
        titleString = StringCache.DirectoryBrowserTitle;
        forcePause = true;
        doCloseX = true;
        doCloseButton = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = true;
        layer = WindowLayer.Super;
    }

    public Dialog_DirectoryBrowser(Action<DirectoryInfo> selAction, string title = null, string rootLimit = null)
    {
        //
        titleString = title ?? StringCache.DirectoryBrowserTitle;
        forcePause = true;
        doCloseX = true;
        doCloseButton = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = true;
        layer = WindowLayer.Super;

        this.selAction = selAction;
        if (rootLimit != null) this.rootLimit = new DirectoryInfo(rootLimit);
    }

    public Dialog_DirectoryBrowser(Action<DirectoryInfo> selAction, string title = null, DirectoryInfo rootLimit = null)
    {
        //
        titleString = title ?? StringCache.DirectoryBrowserTitle;
        forcePause = true;
        doCloseX = true;
        doCloseButton = false;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = true;
        layer = WindowLayer.Super;

        this.selAction = selAction;
        this.rootLimit = rootLimit;
    }

    public override Vector2 InitialSize => new(1000, 500);

    public override void PostOpen()
    {
        base.PostOpen();
        //
        if (rootLimit == null)
        {
            TLog.Warning("Opened Directory Selection Without Setting Root");
            rootLimit = new DirectoryInfo(GenFilePaths.ModsFolderPath);
            currentDir = new DirectoryInfo(GenFilePaths.ModsFolderPath);
            return;
        }

        currentDir = new DirectoryInfo(rootLimit.FullName);
    }

    public override void DoWindowContents(Rect inRect)
    {
        var titleRect = inRect.TopPart(0.10f);
        var selectionRect = inRect.BottomPart(.90f);
        var selRect = selectionRect.ContractedBy(5).Rounded();
        var directoryRect = selRect.TopPartPixels(selRect.height - 30);
        var confirmButton = selRect.BottomPartPixels(30).RightPartPixels(100);

        Text.Font = GameFont.Medium;
        Widgets.Label(titleRect, titleString);
        Text.Font = default;

        DrawDirectory(directoryRect, currentDir);

        //Confirm button
        if (Widgets.ButtonText(confirmButton, "Confirm"))
        {
            selAction?.Invoke(currentDir);
            Close();
        }
    }

    internal void DrawDirectory(Rect inRect, DirectoryInfo directory)
    {
        var subDirs = directory.GetDirectories();
        //
        var conRect = inRect.ContractedBy(8);
        var topRect = conRect.TopPartPixels(50);
        var extraRect = topRect.TopPartPixels(25);
        var curDirReadoutRect = topRect.BottomPartPixels(25).ExpandedBy(8, 0);
        var selectionRect = inRect.BottomPartPixels(inRect.height - topRect.height);

        var navBackRect = curDirReadoutRect.LeftPartPixels(curDirReadoutRect.height);
        var navCurRect = new Rect(navBackRect.xMax + 5, navBackRect.y, topRect.width - navBackRect.width,
            curDirReadoutRect.height);

        TWidgets.DrawColoredBox(selectionRect, TColor.BlueHueBG, Color.gray, 1);
        selectionRect = selectionRect.ContractedBy(8);

        TWidgets.DrawColoredBox(curDirReadoutRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);
        GUI.color = TColor.MenuSectionBGBorderColor;
        Widgets.DrawLineVertical(navBackRect.xMax + 2, navBackRect.y, navBackRect.height);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(extraRect, $"Root: {rootLimit.Parent?.Name}\\{rootLimit.Name}");
        Widgets.Label(navCurRect, $"{ModDirectoryData.PathCapped(currentDir, rootLimit)}");
        Text.Anchor = default;
        if (currentDir.Name != rootLimit.Name)
            if (Widgets.ButtonImage(navBackRect, TeleContent.OpenMenu))
                currentDir = directory.Parent;

        Widgets.BeginGroup(selectionRect);
        {
            var curY = 0;
            foreach (var subDir in subDirs)
            {
                var dirSelRect = new Rect(0, curY, selectionRect.width, 20);
                Widgets.Label(dirSelRect, $"{subDir.Name}");

                Widgets.DrawHighlightIfMouseover(dirSelRect);
                if (Widgets.ButtonInvisible(dirSelRect, false)) currentDir = subDir;
                curY += 20;
            }
        }
        Widgets.EndGroup();
    }
}