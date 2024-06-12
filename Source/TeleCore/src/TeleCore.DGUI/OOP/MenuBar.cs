using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.DGUI.OOP;

public class MenuOption
{
    private Action _onClick;
    
    public void Draw()
    {
        
    }
}

public class MenuButton
{
    private List<MenuOption> _options;
    private Action _onClick; //<- technically this could make an agnostic button that opens a dropdown with an action

    //Can have toggle state with options
    //Or single click action without options
    public MenuButton()
    {
        _options = new List<MenuOption>();
    }

    public void Draw()
    {
        //Draw Button
        
        //If button opened and has options
        bool opened = true;
        bool hasOptions = _options.Count > 0;
        if (opened && hasOptions)
        {
            foreach (var option in _options)
            {
                option.Draw();
            }
        }
    }
}

public class MenuBar
{
    private List<MenuButton> _buttons;

    public MenuBar()
    {
        _buttons = new List<MenuButton>();
    }

    public MenuButton RegisterButton(string label, Action directAction = null)
    {
        var button = new MenuButton();
        _buttons.Add(button);
        return button;
    }
    
    public void Draw()
    {
        //Draw MenuBar Shape / BG
        
        //Draw Buttons
        foreach (var button in _buttons)
        {
            button.Draw();
        }
    }
}

public static class Implementation
{
    public static MenuBar bar;

    static Implementation()
    {
        bar = new MenuBar();
        bar.RegisterButton("File");
        bar.RegisterButton("Edit");
        //Etc..
    }
    
    public static void DrawMenuBar()
    {
        bar.Draw();
    }
}