namespace GeneticAlgorithms

type Problem<'Gene> =
    { genotype: unit -> Chromosome<'Gene>
      fitness_function: Chromosome<'Gene> -> float
      terminate: seq<Chromosome<'Gene>> -> bool }
