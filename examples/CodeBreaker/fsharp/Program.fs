open System
open GeneticAlgorithms

let target = "ILoveGeneticAlgorithms"
let encrypted = "LIjs`B`k`qlfDibjwlqmhv"

let cipher (text: string) key =
    text
    |> Seq.map (fun character -> char ((int character ^^^ key) % 32768))
    |> Seq.toArray
    |> String

let chromosomeKey (chromosome: Chromosome<int>) =
    chromosome.Genes
    |> Array.fold (fun key gene -> (key <<< 1) ||| gene) 0

let genotype () =
    { Genes = Array.init target.Length (fun _ -> Random.Shared.Next(0, 2))
      Fitness = 0.0
      Age = 0 }

let fitnessFunction (chromosome: Chromosome<int>) =
    chromosome
    |> chromosomeKey
    |> cipher encrypted
    |> Distance.jaroSimilarity target

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= 1.0)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitnessFunction
      Terminate = terminate }

let options =
    { PopulationSize = 100
      SelectionRate = 1.0
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      Probe = Probes.printProgress }

let solution = Genetic.run problem options
let key = chromosomeKey solution
let decoded = cipher encrypted key

printfn "Target:   %s" target
printfn "Decoded:  %s" decoded
printfn "Key:      %d (%A)" key solution.Genes
printfn "Fitness:  %f" solution.Fitness
