﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Verse;

namespace TeleCore.Primitive;

[StaticConstructorOnStartup]
public static class NumericLibrary<T> where T : unmanaged
{
    public static readonly Func<T, T, T> Addition;
    public static readonly Func<T, T, T> Subtraction;
    public static readonly Func<T, T, T> Multiplication;
    public static readonly Func<T, T, T> Division;

    public static readonly Func<T, T, bool> GreaterThan;
    public static readonly Func<T, T, bool> LessThan;
    public static readonly Func<T, T, bool> Equal;
    public static readonly Func<T, T, bool> NotEqual;
    public static readonly Func<T, T, bool> GreaterThanOrEqual;
    public static readonly Func<T, T, bool> LessThanOrEqual;

    public static readonly Func<T> EpsilonGetter;
    public static readonly Func<T> ZeroGetter;
    public static readonly Func<T> OneGetter;
    public static readonly Func<T> NegativeOneGetter;
    public static readonly Func<T> NaNGetter;

    public static readonly Func<IEnumerable<T>, T> Sum;
    public static readonly Func<T, T, T, T> Clamp;
    public static readonly Func<T, int, T> Round;
    public static readonly Func<T, T, T> Min;
    public static readonly Func<T, T, T> Max;
    public static readonly Func<T, T> Abs;

    static NumericLibrary()
    {
        Addition = CreateAddFunc();
        Subtraction = CreateSubtractFunc();
        Multiplication = CreateMultiplicationFunc();
        Division = CreateDivisionFunc();

        GreaterThan = CreateGreaterThan();
        LessThan = CreateLessThan();
        Equal = CreateEqual();
        NotEqual = CreateNotEqual();
        GreaterThanOrEqual = CreateGreateThanOrEqual();
        LessThanOrEqual = CreateLessThanOrEqual();

        EpsilonGetter = CreateEpsilonGetter();
        ZeroGetter = CreateZeroGetter();
        OneGetter = CreateOneGetter();
        NegativeOneGetter = CreateNegativeOneGetter();
        NaNGetter = CreateNaNGetter();

        Sum = CreateSumFunc();
        Min = CreateMinFunc();
        Max = CreateMaxFunc();
        Clamp = CreateClampFunc();
        Round = CreateRoundFunc();
        Abs = CreateAbsFunc();
    }

    public static Numeric<T> Zero => new(ZeroGetter());

    private static Func<T, T, T> CreateAddFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Add(paramA, paramB);
        var add = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return add.Compile();
    }

    private static Func<T, T, T> CreateSubtractFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Subtract(paramA, paramB);
        var sub = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return sub.Compile();
    }

    private static Func<T, T, T> CreateMultiplicationFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Multiply(paramA, paramB);
        var mult = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return mult.Compile();
    }

    private static Func<T, T, T> CreateDivisionFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Divide(paramA, paramB);
        var div = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return div.Compile();
    }

    private static Func<T, T, bool> CreateGreaterThan()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.GreaterThan(paramA, paramB);
        var gt = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return gt.Compile();
    }

    private static Func<T, T, bool> CreateLessThan()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.LessThan(paramA, paramB);
        var lt = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return lt.Compile();
    }

    private static Func<T, T, bool> CreateEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Equal(paramA, paramB);
        var eq = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return eq.Compile();
    }

    private static Func<T, T, bool> CreateNotEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.NotEqual(paramA, paramB);
        var neq = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return neq.Compile();
    }

    private static Func<T, T, bool> CreateGreateThanOrEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.GreaterThanOrEqual(paramA, paramB);
        var gteq = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return gteq.Compile();
    }

    private static Func<T, T, bool> CreateLessThanOrEqual()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.LessThanOrEqual(paramA, paramB);
        var lteq = Expression.Lambda<Func<T, T, bool>>(body, paramA, paramB);
        return lteq.Compile();
    }

    private static Func<T> CreateZeroGetter()
    {
        var zeroConstant = Expression.Constant(0);
        var zero = Expression.Convert(zeroConstant, typeof(T));
        var lambda = Expression.Lambda<Func<T>>(zero);
        return lambda.Compile();
    }

    private static Func<T> CreateOneGetter()
    {
        var oneConst = Expression.Constant(1);
        var one = Expression.Convert(oneConst, typeof(T));
        var lambda = Expression.Lambda<Func<T>>(one);
        return lambda.Compile();
    }

    private static Func<T> CreateNegativeOneGetter()
    {
        var oneConst = Expression.Constant(-1);
        var one = Expression.Convert(oneConst, typeof(T));
        var lambda = Expression.Lambda<Func<T>>(one);
        return lambda.Compile();
    }

    public static Func<T> CreateNaNGetter()
    {
        ConstantExpression constant;

        if (typeof(T) == typeof(float))
        {
            constant = Expression.Constant(float.NaN);
        }
        else if (typeof(T) == typeof(double))
        {
            constant = Expression.Constant(double.NaN);
        }
        else
        {
            constant = Expression.Constant(0); // For integer types just use 0
        }

        var nan = Expression.Convert(constant, typeof(T));
        var lambda = Expression.Lambda<Func<T>>(nan);
        return lambda.Compile();
    }

    private static Func<IEnumerable<T>, T> CreateSumFunc()
    {
        var itemsExpr = Expression.Parameter(typeof(IEnumerable<T>), "items");
        var enumItemType = typeof(IEnumerable<>).MakeGenericType(typeof(T));

        // Get the appropriate Sum method
        MethodInfo sumMethod = null;

        //
        var methods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static);
        var methods2 = methods.Where(m => m.Name == "Sum");
        var methods3 = methods2.Where(m => m.ReturnType == typeof(T));
        sumMethod = methods3.FirstOrDefault(m =>
                                    m.GetParameters().Length == 1
                                 && m.GetParameters()[0].ParameterType == enumItemType)!;


        if (sumMethod == null)
        {
            throw new NotSupportedException("Unable to find appropriate Sum method.");
        }

        var body = Expression.Call(null, sumMethod, itemsExpr);
        var sumFunc = Expression.Lambda<Func<IEnumerable<T>, T>>(body, itemsExpr);
        return sumFunc.Compile();
    }

    private static Func<T, T, T, T> CreateClampFunc()
    {
        var value = Expression.Parameter(typeof(T), "value");
        var min = Expression.Parameter(typeof(T), "min");
        var max = Expression.Parameter(typeof(T), "max");

        var body = Expression.Condition(
            Expression.LessThan(value, min), min,
            Expression.Condition(
                Expression.GreaterThan(value, max), max, value));

        var clamp = Expression.Lambda<Func<T, T, T, T>>(body, value, min, max);

        return clamp.Compile();
    }

    private static Func<T, int, T> CreateRoundFunc()
    {
        if (typeof(T) != typeof(double) && typeof(T) != typeof(float) && typeof(T) != typeof(decimal))
        {
            return (toRound, decimals) => toRound;
        }

        var value = Expression.Parameter(typeof(T), "value");
        var decimals = Expression.Parameter(typeof(int), "decimals");

        if (typeof(T) == typeof(float))
        {
            var converted = Expression.Convert(value, typeof(double));
            var roundMethod = typeof(Math).GetMethod("Round", new[] { typeof(double), typeof(int) });
            var body = Expression.Call(roundMethod, converted, decimals);
            var roundConvert = Expression.Convert(body, typeof(float));
            var round = Expression.Lambda<Func<T, int, T>>(roundConvert, value, decimals);

            return round.Compile();
        }
        else
        {
            var roundMethod = typeof(Math).GetMethod("Round", new[] { typeof(T), typeof(int) });
            var body = Expression.Call(roundMethod, value, decimals);
            var round = Expression.Lambda<Func<T, int, T>>(body, value, decimals);

            return round.Compile();
        }
    }

    private static Func<T, T, T> CreateMinFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Condition(Expression.LessThan(paramA, paramB), paramA, paramB);
        var min = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return min.Compile();
    }

    private static Func<T, T, T> CreateMaxFunc()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");
        var body = Expression.Condition(Expression.GreaterThan(paramA, paramB), paramA, paramB);
        var max = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB);
        return max.Compile();
    }

    private static Func<T, T> CreateAbsFunc()
    {
        var valueParam = Expression.Parameter(typeof(T), "value");
        var zeroConst = Expression.Constant(0);
        var zero = Expression.Convert(zeroConst, typeof(T));
        var body = Expression.Condition(Expression.LessThan(valueParam, zero),
            Expression.Negate(valueParam),
            valueParam);
        var lambda = Expression.Lambda<Func<T, T>>(body, valueParam);
        return lambda.Compile();
    }

    private static Func<T> CreateEpsilonGetter()
    {
        object epsilon;

        if (typeof(T) == typeof(float))
        {
            epsilon = float.Epsilon;
        }
        else if (typeof(T) == typeof(double))
        {
            epsilon = double.Epsilon;
        }
        else if (typeof(T) == typeof(decimal))
        {
            epsilon = (decimal)double.Epsilon; // Convert float.Epsilon to decimal type
        }
        else
        {
            epsilon = 0; // For integer types just use 0
        }

        var constant = Expression.Constant(epsilon, typeof(T));
        var body = Expression.Convert(constant, typeof(T));  // necessary due to boxing of value types
        var lambda = Expression.Lambda<Func<T>>(body);
        return lambda.Compile();
    }
}