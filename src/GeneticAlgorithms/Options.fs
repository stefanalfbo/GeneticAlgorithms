namespace GeneticAlgorithms

type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      CrossoverFn: Chromosome<'Gene> -> Chromosome<'Gene> -> Chromosome<'Gene> * Chromosome<'Gene>
      MutationRate: float
      MutationFn: Chromosome<'Gene> -> Chromosome<'Gene>
      OnGeneration: Chromosome<'Gene> -> int -> unit }
