using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace InductiveVariadics;

public static class ree
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}

[Generator]
public sealed class VariadicGenerator : ISourceGenerator
{
    internal record struct Sequence<T>(IEnumerable<T> Seq) where T : notnull
    {
        public bool Equals(Sequence<T> other)
        {
            return Seq.SequenceEqual(other.Seq);
        }

        // TODO
        public override int GetHashCode()
        {
            var sum = 0;
            foreach (var el in Seq)
                sum += el.GetHashCode();
            return sum;
        }

    }

    internal sealed class InductionBuilder
    {
        public (ITypeSymbol, IMethodSymbol)? Base { get; set; }
        public IMethodSymbol? Transition { get; set; }
        public (ITypeSymbol, IMethodSymbol)? Finalization { get; set; }

        public Induction ToInduction()
            => new(Base ?? throw new(), Transition ?? throw new(), Finalization ?? throw new());
    }

    internal record struct Induction((ITypeSymbol, IMethodSymbol) Base, IMethodSymbol Transition, (ITypeSymbol, IMethodSymbol) Finalization);

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (SyntaxReceiver)context.SyntaxContextReceiver!;
        var variadicMethodBuilder = new Dictionary<string, InductionBuilder>();

        foreach (var methodDecl in receiver!.MethodsWithAttributes)
        {
            var model = context.Compilation.GetSemanticModel(methodDecl.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(methodDecl);

            if (symbol is IMethodSymbol calledSymbol)
            {
                var methodSymbol = calledSymbol.OriginalDefinition;
                var attr = symbol.GetAttributes().SingleOrDefault(c =>
                    c.AttributeClass!.Name.StartsWith("Induction"));
                var name = attr?.ConstructorArguments[0].Value?.ToString();
                var builder = name switch
                {
                    { } when variadicMethodBuilder.TryGetValue(name, out var existing) => existing,
                    { } => variadicMethodBuilder[name] = new(),
                    _ => null
                };
                switch (attr)
                {
                    case { AttributeClass.Name: "InductionBaseOfAttribute" }:
                        builder!.Base = (methodSymbol.ReturnType, methodSymbol);
                        break;
                    case { AttributeClass.Name: "InductionTransitionOfAttribute" }:
                        builder!.Transition = methodSymbol;
                        break;
                    case { AttributeClass.Name: "InductionFinalizationOfAttribute" }:
                        builder!.Finalization = (methodSymbol.ReturnType, methodSymbol);
                        break;
                }
            }
        }

        var calls = new Dictionary<(string Name, Sequence<ITypeSymbol>?, int Arity), Induction>();

        foreach (var invocation in receiver!.Invocations)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                if (variadicMethodBuilder.TryGetValue(memberAccess.Name.Identifier.ToString(), out var builder))
                {
                    var induction = builder.ToInduction();
                    Sequence<ITypeSymbol>? types = null;
                    if (!induction.Transition.IsGenericMethod)
                    {
                        var model = context.Compilation.GetSemanticModel(invocation.ArgumentList.Arguments[0].SyntaxTree);
                        var operation = model.GetOperation(invocation);
                        types = new(operation?.Children.Skip(1).Select(c => c.Type ?? throw new("It's null!!!")) ?? throw new("It's null!!!"));
                    }
                    var count = invocation.ArgumentList.Arguments.Count;
                    calls[(memberAccess.Name.Identifier.ToString(), types, count)] = induction;
                }
        }

        foreach (var ((name, types, arity), ((baseType, baseMethod), transition, (finalType, finalMethod))) in calls)
        {
            var classType = baseMethod.ContainingType;

            var typeParameters = 
                transition.IsGenericMethod
                ? ("<" + string.Join(", ",
                Enumerable.Range(1, arity)
                    .Select(x => $"T{x}")) + ">")
                : "";

            var parameters = "";
            if (transition.IsGenericMethod)
            {
                parameters = string.Join(", ", Enumerable.Range(1, arity).Select(x => $"T{x} value{x}"));
            }
            else if (types is { } validTypes)
            {
                parameters = string.Join(", ", validTypes.Seq.Select((x, i) => $"{x.ToDisplayString()} value{i + 1}"));
            }

            var steps =
                Enumerable.Range(1, arity)
                .Select(c => $"value = {transition.Name}(value{c}, value);\n");
            var src =
$@"
partial class {classType.Name}
{{
    public static {finalType.ToDisplayString()} {name}{typeParameters}({parameters})
    {{
        var value = {baseMethod.Name}();
        {string.Join("", steps)}
        return {finalMethod.Name}(value);
    }}
}}
";
            if (!classType.ContainingNamespace.IsGlobalNamespace)
                src = $"namespace {classType.ContainingNamespace}\n{{\n{src}\n}}";
            context.AddSource($"VariadicMethod.{name}.{arity}.cs", src);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
         #if DEBUG && false
         if (!Debugger.IsAttached)
         {
             Debugger.Launch();
         }
         #endif 
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private sealed class SyntaxReceiver : ISyntaxContextReceiver
    {
        private readonly List<InvocationExpressionSyntax> _invocations;
        private readonly List<MethodDeclarationSyntax> _methodsWithInductionAttributes;

        public SyntaxReceiver()
        {
            _invocations = new();
            _methodsWithInductionAttributes = new();
        }

        public IReadOnlyList<InvocationExpressionSyntax> Invocations
            => _invocations;

        public IReadOnlyList<MethodDeclarationSyntax> MethodsWithAttributes
            => _methodsWithInductionAttributes;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax args)
            {
                _invocations.Add(args);
            }
            else if (context.Node is MethodDeclarationSyntax method && method.AttributeLists.Count > 0)
            {
                _methodsWithInductionAttributes.Add(method);
            }
        }
    }
}
