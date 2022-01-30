// See https://aka.ms/new-console-template for more information

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using InductiveVariadics;


Console.WriteLine(
    Stuff.Concat(1, 2.56f, "aaa", 4343.3)
    );



Console.WriteLine(Arithmetics.Add(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
Console.WriteLine();
Console.WriteLine(Sizes.CountSizes(0, 1.2, 3.3m));

Sum(1, 2, 3, 4);
// equivalent to
Sum(new int[] { 1, 2, 3, 4 });

static int Sum(params int[] objects)
    => objects.Sum();

// BenchmarkRunner.Run<Bench>();
// Console.WriteLine(new Bench().AddParams());
// Console.WriteLine(new Bench().AddVariadics());



