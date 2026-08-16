open GeneticAlgorithms

let n = 8
let max_fitness = float n

let genotype () =
    let genes = Array.init n id |> Array.sortBy (fun _ -> System.Random.Shared.Next())

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    let genes = chromosome.genes

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
    population |> Seq.exists (fun chromosome -> chromosome.fitness >= max_fitness)

let problem: Problem<int> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options =
    { population_size = 100
      selection_rate = 0.8
      selection_fn = Selection.elite }

let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f / %f)" solution.genes solution.fitness max_fitness
