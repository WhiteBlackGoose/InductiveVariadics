open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type FoldStep<'State, 'T> = ('State -> 'T -> 'State) -> 'State -> 'State

type FoldBuilder() =
    member this.Zero(): FoldStep<'State, 'T> = 
        fun _ state -> state
    
    member inline this.Combine(
        [<InlineIfLambda>] f: FoldStep<'State, 'T>, 
        [<InlineIfLambda>] g: FoldStep<'State, 'T>)
        : FoldStep<'State, 'T> =
        fun folder state -> g folder (f folder state)
    
    member inline this.Delay([<InlineIfLambda>] f: unit -> FoldStep<'State, 'T>) = 
        f()
    
    member inline this.Yield(value: 'T): FoldStep<'State, 'T> = 
        fun folder state -> folder state value

type Bench() =
    let fold = FoldBuilder()

    [<Benchmark>]
    member _.addVariadics () =
        (fold { 1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 1; 2; 3; 4; 5; 6; 7; 8; 9; 10 }) (+) 0



// (Bench ()).addVariadics() |> printfn "%i"

BenchmarkRunner.Run<Bench>() |> ignore

