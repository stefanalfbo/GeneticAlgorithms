namespace GeneticAlgorithms

module Crossover =

    let orderOneCrossover (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let lim = p1.Genes.Length - 1

        let i1, i2 =
            let a = System.Random.Shared.Next(1, lim + 1)
            let b = System.Random.Shared.Next(1, lim + 1)
            if a <= b then a, b else b, a

        let slice1 = p1.Genes.[i1..i2]
        let slice1Set = System.Collections.Generic.HashSet<'Gene>(slice1)
        let p2Contrib = p2.Genes |> Array.filter (slice1Set.Contains >> not)
        let head1, tail1 = Array.splitAt i1 p2Contrib

        let slice2 = p2.Genes.[i1..i2]
        let slice2Set = System.Collections.Generic.HashSet<'Gene>(slice2)
        let p1Contrib = p1.Genes |> Array.filter (slice2Set.Contains >> not)
        let head2, tail2 = Array.splitAt i1 p1Contrib

        { p1 with
            Genes = Array.concat [ head1; slice1; tail1 ] },
        { p2 with
            Genes = Array.concat [ head2; slice2; tail2 ] }
