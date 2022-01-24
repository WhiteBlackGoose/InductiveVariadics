// See https://aka.ms/new-console-template for more information

using System;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using InductiveVariadics;


// Console.WriteLine(Stuff.Concat(1, 2.56f, "aaa", 4343));
// Console.WriteLine(new Bench().StringInteropation());
// Console.WriteLine(new Bench().VariadicConcat());
BenchmarkRunner.Run<Bench>();
// Console.WriteLine(Arithmetics.Add(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
// Console.WriteLine(Arithmetics.AddNaive(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
// Console.WriteLine(new Bench().AddParams());
// Console.WriteLine(new Bench().AddVariadics());

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

partial class Stuff
{
    [InductionBaseOf("Concat")]
    public static StringBuilder ConcatBase() => new(200);



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



    [InductionFinalizationOf("Concat")]
    public static string ConcatFinalize(StringBuilder sb) => sb.ToString();
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