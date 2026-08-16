namespace GeneticAlgorithms

type Problem<'Gene> =
    { Genotype: unit -> Chromosome<'Gene>
      FitnessFunction: Chromosome<'Gene> -> float
      Terminate: seq<Chromosome<'Gene>> -> int -> float -> bool }
