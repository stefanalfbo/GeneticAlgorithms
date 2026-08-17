namespace GeneticAlgorithms

type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      CrossoverFn: Chromosome<'Gene> -> Chromosome<'Gene> -> Chromosome<'Gene> * Chromosome<'Gene>
      MutationRate: float
      OnGeneration: Chromosome<'Gene> -> int -> unit }
