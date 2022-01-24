> **Warning**: this is no more than a proof of concept repo

# Inductive Variadics

Variadic arguments for C# with inductive declaration. Made via C# 9's source generators.

Many heard of variadic arguments in C++. In my IVG PoC (proof of concept of inductive variadic generics) the idea 
is that for a sequence of parameters and types it is in fact a base, a transition from `n` to `n + 1`, and finalization (post-processing).

So, what I mean is, assume you have this:

```cs
var concatenated = Concat(123, "aaaa", 3.5);
```

So by default there's a default string builder (even for 0 args):

```cs
[InductionBaseOf("Concat")]
public static StringBuilder ConcatBase()
    => new();
```

At this point we just create an instance.

Now, given that we already have some `f(n)`, like in math, we define `f(n + 1)`:

```cs
[InductionTransitionOf("Concat")]
public static StringBuilder ConcatTransition<T>(T value, StringBuilder folded)
{
    if (typeof(T) == typeof(int))
        folded.Append((int)((object)value!)!);
    else if (typeof(T) == typeof(float))
        folded.Append((float)((object)value!)!);
    else
        folded.Append(value);
    return folded;
}
```

So we get a new `value` of some type, and already processed `StringBuilder`.

Finally, to get a string out of the string builder, we finalize:

```cs
[InductionFinalizationOf("Concat")]
public static string ConcatFinalize(StringBuilder sb)
    => sb.ToString();
```

Now, when you write
```cs
var concatenated = Concat(123, "aaaa", 3.5);
```

The following code is generated:
```cs
public static string Concat<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
{
   var value = ConcatBase();
   value = ConcatTransition<T1>(value1, value);
   value = ConcatTransition<T2>(value2, value);
   value = ConcatTransition<T3>(value3, value);
   return ConcatFinalize(value);
}
```

Voila!


## Examples


### Add integers

The simplest case to add n integers.

```cs
Console.WriteLine(Arithmetics.Add(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));

partial class Arithmetics
{
    [InductionBaseOf("Add")]
    public static int AddBase() => 0;

    [InductionTransitionOf("Add")]
    public static int AddTransition(int head, int folded) => head + folded;

    [InductionFinalizationOf("Add")]
    public static int AddFinalization(int folded) => folded;
}
```

If we compare that to
```cs
public static int AddNaive(params int[] ints)
{
    var a = 0;
    foreach (var i in ints)
        a += i;
    return a;
}
```
we get:


|       Method |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
|------------- |----------:|----------:|----------:|-------:|----------:|
| AddVariadics |  8.241 ns | 0.2033 ns | 0.4199 ns |      - |         - |
|    AddParams | 28.389 ns | 0.6595 ns | 1.9133 ns | 0.0459 |     144 B |

### Concat objects

To avoid boxing we want to call StringBuilder's overloads for int, double, etc.

So we make multiple overloads for transition.

```cs
Console.WriteLine(Stuff.Concat(1, 2.56f, "aaa", 4343));

partial class Stuff
{
    [InductionBaseOf("Concat")]
    public static StringBuilder ConcatBase() => new(200);



    [InductionTransitionOf("Concat")]
    public static StringBuilder ConcatTransition(string value, StringBuilder folded)
        => folded.Append(value);

    [InductionTransitionOf("Concat")]
    public static StringBuilder ConcatTransition(int value, StringBuilder folded)
        => folded.Append(value);

    [InductionTransitionOf("Concat")]
    public static StringBuilder ConcatTransition(float value, StringBuilder folded)
        => folded.Append(value);

    [InductionTransitionOf("Concat")]
    public static StringBuilder ConcatTransition(double value, StringBuilder folded)
        => folded.Append(value);



    [InductionFinalizationOf("Concat")]
    public static string ConcatFinalize(StringBuilder sb) => sb.ToString();
}
```

### Sum sizes of objects and print them

Finally, the most advanced case is to make transition generic.

```cs
Console.WriteLine(Sizes.CountSizes(0, 1.2, 3.3m));

partial class Sizes
{
    [InductionBaseOf("CountSizes")]
    public static int CountBase() => 0;


    [InductionTransitionOf("CountSizes")]
    public static int CountTransition<T>(T value, int folded)
    {
        System.Console.WriteLine($"Size of {value} is {Marshal.SizeOf<T>()}");
        return Marshal.SizeOf<T>() + folded;
    }

    [InductionFinalizationOf("CountSizes")]
    public static int CountFinalization(int folded) => folded;
}

}
```

## Inspiration

This proof of concept is inspired by [**this**](https://github.com/FiniteReality/VariadicGenerics) project by [FiniteReality](https://github.com/FiniteReality).
