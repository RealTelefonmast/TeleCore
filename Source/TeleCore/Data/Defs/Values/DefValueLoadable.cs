﻿using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TeleCore;

public class DefValueLoadable<TDef, TValue> : IExposable
    where TDef : Def
    where TValue : struct
{
    public TDef def;
    public TValue value;

    public TDef Def => def;

    public TValue Value
    {
        get => value;
        set => this.value = value;
    }

    public bool IsValid => def != null && value is float or int;

    public DefValueLoadable(){ }

    public DefValueLoadable(TDef def, TValue value) : this()
    {
        this.def = def;
        this.value = value;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //Listing
        if (xmlRoot.Name == "li")
        {
            var innerValue = xmlRoot.InnerText;
            string s = Regex.Replace(innerValue, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, $"{nameof(def)}", array[0]);
            value = ParseHelper.FromString<TValue>(array.Length > 1 ? array[1] : "1");
        }

        //InLined
        else
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, $"{nameof(def)}", xmlRoot.Name);
            value = ParseHelper.FromString<TValue>(xmlRoot.FirstChild.Value);
        }
    }

    public override string ToString()
    {
        return $"{def?.defName}: {value}";
    }

    public void ExposeData()
    {
        Look<TDef>(ref def, "def");
        Scribe_Values.Look(ref value, nameof(value));
    }
    
    public static void Look<T>(ref T value, string label) where T : Def
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            string text;
            text = value == null ? "null" : value.defName;
            Scribe_Values.Look<string>(ref text, label, "null", false);
            return;
        }
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            value = ScribeExtractor.DefFromNodeUnsafe<T>(Scribe.loader.curXmlParent[label]);
        }
    }
}