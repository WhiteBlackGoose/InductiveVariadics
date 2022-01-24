// See https://aka.ms/new-console-template for more information

using System;
using System.Text;
using InductiveVariadics;


Console.WriteLine(Stuff.Concat(1, 2.56f, "aaa", 4343));

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
