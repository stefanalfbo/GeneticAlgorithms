namespace GeneticAlgorithms

type Problem<'Gene> =
    { Genotype: System.Random -> Chromosome<'Gene>
      FitnessFunction: Chromosome<'Gene> -> float
      Terminate: seq<Chromosome<'Gene>> -> int -> float -> bool }
