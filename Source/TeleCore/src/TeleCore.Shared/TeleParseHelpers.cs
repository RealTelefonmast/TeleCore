﻿using Verse;

namespace TeleCore.Shared;

[StaticConstructorOnStartup]
public class TeleParseHelpers
{
    static TeleParseHelpers()
    {
        ParseHelper.Parsers<Numeric<int>>.Register(ParseInt);
        ParseHelper.Parsers<Numeric<float>>.Register(ParseFloat);
        ParseHelper.Parsers<Numeric<double>>.Register(ParseDouble);
    }
    
    private static Numeric<double> ParseDouble(string arg)
    {
        var val = ParseHelper.ParseDouble(arg);
        return new Numeric<double>(val);
    }

    private static Numeric<float> ParseFloat(string arg)
    {
        var val = ParseHelper.ParseFloat(arg);
        return new Numeric<float>(val);
    }

    private static Numeric<int> ParseInt(string arg)
    {
        var val = ParseHelper.ParseIntPermissive(arg);
        return new Numeric<int>(val);
    }

}