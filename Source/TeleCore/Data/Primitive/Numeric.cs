﻿
namespace TeleCore.Primitive;

public struct Numeric<TValue> where TValue : unmanaged
{
    private TValue _number;
    
    public bool IsZero => this == NumericLibrary<TValue>.ZeroGetter()!;
    public TValue Value => _number;

    public static Numeric<TValue> Epsilon => new(NumericLibrary<TValue>.EpsilonGetter());
    public static Numeric<TValue> Zero => new(NumericLibrary<TValue>.ZeroGetter());
    public static Numeric<TValue> One => new(NumericLibrary<TValue>.OneGetter());
    public static Numeric<TValue> NegativeOne => new(NumericLibrary<TValue>.NegativeOneGetter());
    public bool IsNaN
    {
        get
        {
            return _number switch
            {
                double d => double.IsNaN(d),
                float f => float.IsNaN(f),
                _ => false
            };
        }
    }

    public Numeric(TValue value)
    {
        _number = value;
    }

    #region Conversion Operators

    public static implicit operator TValue(Numeric<TValue> numeric)
    {
        return numeric.Value;
    }

    public static implicit operator Numeric<TValue>(TValue value)
    {
        return new Numeric<TValue>(value);
    }

    #endregion


    #region Numeric Math

    public static Numeric<TValue> operator +(Numeric<TValue> num, TValue value)
    {
        return new Numeric<TValue>(NumericLibrary<TValue>.Addition(num.Value, value));
    }

    public static Numeric<TValue> operator -(Numeric<TValue> num, TValue value)
    {
        return new Numeric<TValue>(NumericLibrary<TValue>.Subtraction(num.Value, value));
    }

    public static Numeric<TValue> operator *(Numeric<TValue> num, TValue value)
    {
        return new Numeric<TValue>(NumericLibrary<TValue>.Multiplication(num.Value, value));
    }

    public static Numeric<TValue> operator /(Numeric<TValue> num, TValue value)
    {
        return new Numeric<TValue>(NumericLibrary<TValue>.Division(num.Value, value));
    }

    #endregion

    #region Comparision

    public static bool operator >(Numeric<TValue> value, Numeric<TValue> num)
    {
        return NumericLibrary<TValue>.GreaterThan(value, num);
    }
    
    public static bool operator <(Numeric<TValue> value, Numeric<TValue> num)
    {
        return NumericLibrary<TValue>.LessThan(value, num);
    }
    
    public static bool operator >(TValue value, Numeric<TValue> num)
    {
        return NumericLibrary<TValue>.GreaterThan(value, num.Value);
    }
    
    public static bool operator <(TValue value, Numeric<TValue> num)
    {
        return NumericLibrary<TValue>.LessThan(value, num.Value);
    }
    
    public static bool operator >(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.GreaterThan(num.Value, value);
    }

    public static bool operator <(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.LessThan(num.Value, value);
    }

    public static bool operator ==(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.Equal(num.Value, value);
    }

    public static bool operator !=(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.NotEqual(num.Value, value);
    }

    public static bool operator >=(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.GreaterThanOrEqual(num.Value, value);
    }

    public static bool operator <=(Numeric<TValue> num, TValue value)
    {
        return NumericLibrary<TValue>.LessThanOrEqual(num.Value, value);
    }

    #endregion

    private bool Equals(Numeric<TValue> other)
    {
        return _number.Equals(other._number);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Numeric<TValue> other && Equals(other);
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
    
    public override int GetHashCode()
    {
        return _number.GetHashCode();
    }
}