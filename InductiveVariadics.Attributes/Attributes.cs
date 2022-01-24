using System;

namespace InductiveVariadics;

public sealed class InductionBaseOfAttribute : Attribute
{
    public string Name { get; }
    public InductionBaseOfAttribute(string name) => Name = name;
}

public sealed class InductionTransitionOfAttribute : Attribute
{
    public string Name { get; }
    public InductionTransitionOfAttribute(string name) => Name = name;
}

public sealed class InductionFinalizationOfAttribute : Attribute
{
    public string Name { get; }
    public InductionFinalizationOfAttribute(string name) => Name = name;
}
