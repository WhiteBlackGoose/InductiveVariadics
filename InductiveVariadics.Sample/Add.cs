using BenchmarkDotNet.Attributes;
using InductiveVariadics;
using System.Runtime.CompilerServices;

partial class Arithmetics
{
    [InductionBaseOf("Add"), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AddBase() => 0;



    [InductionTransitionOf("Add"), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AddTransition(int head, int folded) => head + folded;



    [InductionFinalizationOf("Add"), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AddFinalization(int folded) => folded;

    public static int AddNaive(params int[] ints)
    {
        var a = 0;
        foreach (var i in ints)
            a += i;
        return a;
    }
}

[MemoryDiagnoser]
public class Bench
{
    [Benchmark]
    public int AddVariadics()
        => Arithmetics.Add(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

    [Benchmark]
    public int AddParams()
        => Arithmetics.AddNaive(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
}