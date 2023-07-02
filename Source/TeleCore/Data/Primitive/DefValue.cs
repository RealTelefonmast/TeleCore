﻿using Verse;

namespace TeleCore.Primitive;

/// <summary>
///     Wraps any <see cref="Def" /> Type into a struct, attaching a numeric value
/// </summary>
/// <typeparam name="TDef">The <see cref="Def" /> Type of the value.</typeparam>
/// <typeparam name="TValue">The numeric Type of the value.</typeparam>
public struct DefValue<TDef, TValue>
    where TDef : Def
    where TValue : struct
{
    public TDef Def { get; }
    public Numeric<TValue> Value { get; set; }

    public static implicit operator DefValue<TDef, TValue>((TDef Def, Numeric<TValue> Value) value) => new(value.Def, value.Value);
    public static implicit operator TDef(DefValue<TDef, TValue> def) => def.Def;
    public static explicit operator Numeric<TValue>(DefValue<TDef, TValue> def) => def.Value;


    public static DefValue<TDef, TValue> Invalid => new(null, Numeric<TValue>.Zero);

    public DefValue(DefValueLoadable<TDef, TValue> defValue)
    {
        Def = defValue.Def;
        Value = defValue.Value;
    }

    public DefValue(TDef def, TValue value)
    {
        Def = def;
        Value = value;
    }

    #region Math

    public static DefValue<TDef, TValue> operator +(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value + b);
    }

    public static DefValue<TDef, TValue> operator -(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value - b);
    }

    public static DefValue<TDef, TValue> operator *(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value * b);
    }

    public static DefValue<TDef, TValue> operator /(DefValue<TDef, TValue> a, TValue b)
    {
        return new DefValue<TDef, TValue>(a.Def, a.Value / b);
    }


    public static DefValue<TDef, TValue> operator +(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def) return Invalid;
        return new DefValue<TDef, TValue>(a.Def, a.Value + b.Value);
    }

    public static DefValue<TDef, TValue> operator -(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def) return Invalid;
        return new DefValue<TDef, TValue>(a.Def, a.Value - b.Value);
    }

    public static DefValue<TDef, TValue> operator *(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def) return Invalid;
        return new DefValue<TDef, TValue>(a.Def, a.Value * b.Value);
    }

    public static DefValue<TDef, TValue> operator /(DefValue<TDef, TValue> a, DefValue<TDef, TValue> b)
    {
        if (a.Def != b.Def) return Invalid;
        return new DefValue<TDef, TValue>(a.Def, a.Value / b.Value);
    }

    #endregion

    #region Comparision

    public static bool operator >(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value > b;
    }

    public static bool operator <(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value < b;
    }


    public static bool operator ==(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value == b;
    }


    public static bool operator !=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value == b;
    }


    public static bool operator >=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value >= b;
    }


    public static bool operator <=(DefValue<TDef, TValue> a, TValue b)
    {
        return a.Value <= b;
    }

    #endregion

    public override string ToString()
    {
        return $"(({Def.GetType()}):{Def}, {Value})";
    }
}