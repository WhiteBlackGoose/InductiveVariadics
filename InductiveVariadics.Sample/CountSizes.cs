
using InductiveVariadics;
using System.Runtime.InteropServices;

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
