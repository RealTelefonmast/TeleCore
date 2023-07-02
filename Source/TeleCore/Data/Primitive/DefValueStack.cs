﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Verse;

namespace TeleCore.Primitive;

/// <summary>
///     Manages any Def as a numeric value in a stack.
/// </summary>
/// <typeparam name="TDef">The <see cref="Def" /> of the stack.</typeparam>
/// <typeparam name="TValue">The numeric type of the stack.</typeparam>
public struct DefValueStack<TDef, TValue> : IExposable
    where TDef : Def
    where TValue : struct
{
    private ImmutableArray<DefValue<TDef, TValue>> _stack;
    private Numeric<TValue> _maxCapacity;
    private Numeric<TValue> _totalValue;

    //States
    public int Length => _stack.Length;
    public Numeric<TValue> TotalValue => _totalValue;
    public bool IsValid => _stack != null;
    public bool Empty => _totalValue.IsZero;

    public bool? Full
    {
        get
        {
            if (!_maxCapacity.IsZero) return null;
            return _totalValue >= _maxCapacity.Value;
        }
    }

    //Stack Info
    public IEnumerable<TDef> Defs => _stack.Select(value => value.Def);
    public ICollection<DefValue<TDef, TValue>> Values => _stack;

    public DefValue<TDef, TValue> this[int index] => _stack[index];

    public DefValue<TDef, TValue> this[TDef def]
    {
        get => TryGetWithFallback(def, new DefValue<TDef, TValue>(def, Numeric<TValue>.Zero));
        private set => TryAddOrSet(value);
    }

    public static DefValueStack<TDef, TValue> Invalid => new()
    {
        _totalValue = Numeric<TValue>.NegativeOne
    };
    
    public DefValueStack()
    {
        _stack = ImmutableArray<DefValue<TDef, TValue>>.Empty;
        _totalValue = Numeric<TValue>.Zero;
    }

    public DefValueStack(TValue maxCapacity) : this()
    {
        _maxCapacity = maxCapacity;
    }

    public DefValueStack(DefValueStack<TDef, TValue> other, TValue maxCapacity) : this(maxCapacity)
    {
        if (!other.IsValid)
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new FlowValueStack from invalid FlowValueStack.");
            return;
        }

        _stack = other._stack;
        _totalValue = other._totalValue;
    }

    public DefValueStack(DefValueStack<TDef,TValue> other) : this(other, Numeric<TValue>.Zero)
    {
    }
    
    /*public DefValueStack(IDictionary<TDef, TValue> source, TValue maxCapacity) : this(maxCapacity)
    {
        if (source.EnumerableNullOrEmpty())
        {
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef,TValue>[source.Count];
        var i = 0;
        foreach (var value in source)
        {
            _stack[i] = new DefValue<TDef>(value.Key, value.Value);
            AdjustTotal(value.Value);
            i++;
        }
    }

    public DefValueStack(ICollection<TDef> defs, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (!defs.Any())
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from Empty {typeof(TDef)} Array.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[defs.Count];
        var i = 0;
        foreach (var def in defs)
        {
            _stack[i] = new DefValue<TDef>(def, 0);
            i++;
        }
    }

    public DefValueStack(DefValue<TDef>[] source, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (source.NullOrEmpty())
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from NullOrEmpty DefFloat Array.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[source.Length];
        var i = 0;
        foreach (var value in source)
        {
            _stack[i] = value;
            AdjustTotal(value.Value);
            i++;
        }
    }

    public DefValueStack(DefValueStack<TDef, TValue> other, Numeric<TValue> maxCapacity) : this(maxCapacity)
    {
        if (!other.IsValid)
        {
            TLog.Warning($"[{GetType()}.ctor]Tried to create new DefValueStack from invalid DefValueStack.");
            Default(maxCapacity);
            return;
        }

        _stack = new DefValue<TDef>[other._stack.Length];
        for (var i = 0; i < _stack.Length; i++)
        {
            _stack[i] = other._stack[i];
        }
        _totalValue = other._totalValue;
    }*/

    public void ExposeData()
    {
        Scribe_Values.Look(ref _maxCapacity, "maxCapacity");
        Scribe_Values.Look(ref _totalValue, "totalValue");
        Scribe_Arrays.Look(ref _stack, "stack");
    }

    private int IndexOf(TDef def)
    {
        for (var i = 0; i < _stack.Length; i++)
            if (_stack[i].Def == def)
                return i;

        return -1;
    }

    public DefValue<TDef, TValue> TryGetWithFallback(TDef key, DefValue<TDef, TValue> fallback)
    {
        return TryGetValue(key, out _, out var value) ? value : fallback;
    }

    public bool TryGetValue(TDef key, out int index, out DefValue<TDef, TValue> value)
    {
        index = -1;
        var tmp = value = new DefValue<TDef, TValue>(key, Numeric<TValue>.Zero);
        for (var i = 0; i < _stack.Length; i++)
        {
            tmp = _stack[i];
            if (tmp.Def != key) continue;
            value = tmp;
            index = i;
            return true;
        }

        return false;
    }

    private void TryAddOrSet(DefValue<TDef, TValue> newValue)
    {
        if (!TryGetValue(newValue.Def, out var index, out var previous))
        {
            //Add onto stack
            _stack = _stack.Add(newValue);
            index = _stack.Length - 1;
        }
        else
        {
            //Set new value
            _stack = _stack.Replace(previous, newValue);
        }

        if (index < 0) return; //Failed to add

        //Get Delta
        var delta = _stack[index] - previous;
        _totalValue += delta.Value;
    }

    public override string ToString()
    {
        if (_stack == null || Length == 0) return "Empty Stack";
        var sb = new StringBuilder();
        sb.AppendLine($"[TOTAL: {TotalValue}]");
        for (var i = 0; i < Length; i++)
        {
            var value = _stack[i];
            sb.AppendLine($"[{i}] {value}");
        }

        return sb.ToString();
    }

    public IEnumerator<DefValue<TDef, TValue>> GetEnumerator()
    {
        return _stack.Cast<DefValue<TDef, TValue>>()
            .GetEnumerator(); //(IEnumerator<DefFloat<TDef>>)_stack.GetEnumerator();
    }

    public void Reset()
    {
        _totalValue = Numeric<TValue>.Zero;
        _stack = _stack.Clear();
    }
    
    //Math

    #region Math
    
    #region Value Math

    public static DefValueStack<TDef, TValue> operator +(DefValueStack<TDef, TValue> a, TValue b)
    {
        foreach (var value in a._stack) 
            a[value.Def] += value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator -(DefValueStack<TDef, TValue> a, TValue b)
    {
        foreach (var value in a._stack) 
            a[value.Def] -= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> a, TValue b)
    {
        foreach (var value in a._stack) 
            a[value.Def] *= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> a, TValue b)
    {
        foreach (var value in a._stack) 
            a[value.Def] /= value;
        return a;
    }

    #endregion

    #region Stack Math

    public static DefValueStack<TDef, TValue> operator +(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        foreach (var value in a._stack) 
            a[value.Def] += value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator -(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        foreach (var value in a._stack) 
            a[value.Def] -= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        foreach (var value in a._stack) 
            a[value.Def] *= value;
        return a;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        foreach (var value in a._stack) 
            a[value.Def] /= value;
        return a;
    }

    #endregion

    #region DefValue Math

    public static DefValueStack<TDef, TValue> operator +(DefValueStack<TDef, TValue> stack,
        DefValue<TDef, TValue> value)
    {
        stack[value.Def] += value;
        return stack;
    }

    public static DefValueStack<TDef, TValue> operator -(DefValueStack<TDef, TValue> stack,
        DefValue<TDef, TValue> value)
    {
        stack[value.Def] -= value;
        return stack;
    }

    public static DefValueStack<TDef, TValue> operator *(DefValueStack<TDef, TValue> stack,
        DefValue<TDef, TValue> value)
    {
        stack[value.Def] *= value;
        return stack;
    }

    public static DefValueStack<TDef, TValue> operator /(DefValueStack<TDef, TValue> stack,
        DefValue<TDef, TValue> value)
    {
        stack[value.Def] /= value;
        return stack;
    }

    #endregion

    #endregion

    #region Comparision

    #region Stack

    public static bool operator >(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue > b.TotalValue;
    }

    public static bool operator <(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue < b.TotalValue;
    }


    public static bool operator ==(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue == b.TotalValue;
    }


    public static bool operator !=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue != b.TotalValue;
    }


    public static bool operator >=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue >= b.TotalValue;
    }


    public static bool operator <=(DefValueStack<TDef, TValue> a, DefValueStack<TDef, TValue> b)
    {
        return a.TotalValue <= b.TotalValue;
    }

    #endregion

    #region Value

    public static bool operator >(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue > b;
    }

    public static bool operator <(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue < b;
    }


    public static bool operator ==(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue == b;
    }


    public static bool operator !=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue != b;
    }


    public static bool operator >=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue >= b;
    }


    public static bool operator <=(DefValueStack<TDef, TValue> a, TValue b)
    {
        return a._totalValue <= b;
    }

    #endregion

    public bool Equals(DefValueStack<TDef, TValue> other)
    {
        return _stack.Equals(other._stack)
               && _maxCapacity == other._maxCapacity
               && _totalValue == other._totalValue;
    }

    public override bool Equals(object? obj)
    {
        return obj is DefValueStack<TDef, TValue> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _maxCapacity.GetHashCode();
            hashCode = (hashCode * 397) ^ _stack.GetHashCode();
            hashCode = (hashCode * 397) ^ _totalValue.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}