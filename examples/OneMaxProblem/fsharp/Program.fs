open GeneticAlgorithms

let max_fitness = 1000.0

let genotype (rng: System.Random) =
    let genes = Array.init 1000 (fun _ -> rng.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) = chromosome.Genes |> Array.sum |> float

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= max_fitness)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

// Options.create's defaults (SelectionRate 0.8 + MutationRate 0.05 + Reinsertion.elitist's
// survivalRate 0.15 summing to 1.0) keep population size stable across however many
// generations it takes to converge - see Options.create's own remarks for why that matters
// here, where this problem's 1000-gene chromosome can take a while to reach the target.
let options =
    { Options.create 100 with
        Probe = Probes.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A" solution.Fitness
