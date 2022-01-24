// See https://aka.ms/new-console-template for more information

using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using InductiveVariadics;


// Console.WriteLine(Stuff.Concat(1, 2.56f, "aaa", 4343));
// Console.WriteLine(new Bench().StringInteropation());
// Console.WriteLine(new Bench().VariadicConcat());
// Console.WriteLine(new Bench().StringInteropation());
BenchmarkRunner.Run<Bench>();

partial class Stuff
{
    [InductionBaseOf("Concat")]
    public static StringBuilder ConcatBase() => new();



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

public class Bench
{
    private int    value1 = 3;
    private float  value2 = 5.5f;
    private string value3 = "hehehe";
    private int    value4 = 24525;
    private float  value5 = 323.2425252f;
    private string value6 = "Ohnoooo";

    [Benchmark]
    public string StringInteropation()
        => $"{value1}{value2}{value3}{value4}{value5}{value6}{value1}{value2}{value3}{value4}{value5}{value6}";

    [Benchmark]
    public string VariadicConcat()
        => Stuff.Concat(value1, value2, value3, value4, value5, value6, value1, value2, value3, value4, value5, value6);
}