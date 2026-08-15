open GeneticAlgorithms

let target = "helloworld"

let randomChar () =
    char (System.Random.Shared.Next(int 'a', int 'z' + 1))

let genotype () =
    let genes = Array.init target.Length (fun _ -> randomChar ())

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<char>) =
    let matches =
        chromosome.genes
        |> Array.indexed
        |> Array.filter (fun (i, gene) -> gene = target.[i])
        |> Array.length

    float matches / float target.Length

let terminate (population: seq<Chromosome<char>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.fitness >= 1.0)

let problem: Problem<char> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 100; selection_rate = 0.8; selection_fn = Selection.elite }

let solution = Genetic.run problem options

printfn "Best solution: %s (fitness: %f)" (System.String solution.genes) solution.fitness
