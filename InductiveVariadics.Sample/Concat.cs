using System.Text;
using InductiveVariadics;

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