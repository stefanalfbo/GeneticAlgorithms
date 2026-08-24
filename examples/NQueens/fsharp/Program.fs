open GeneticAlgorithms

let n = 8
let max_fitness = float n

let genotype () =
    let genes = Array.init n id |> Array.sortBy (fun _ -> System.Random.Shared.Next())

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    let genes = chromosome.Genes

    let diag_clashes =
        [ for i in 0 .. n - 1 do
              for j in 0 .. n - 1 do
                  if i <> j then
                      let dx = abs (i - j)
                      let dy = abs (genes.[i] - genes.[j])
                      if dx = dy then yield 1 else yield 0
                  else
                      yield 0 ]
        |> List.sum

    let distinct_genes = genes |> Array.distinct |> Array.length

    float (distinct_genes - diag_clashes)

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= max_fitness)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.orderOneCrossover
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f / %f)" solution.Genes solution.Fitness max_fitness
