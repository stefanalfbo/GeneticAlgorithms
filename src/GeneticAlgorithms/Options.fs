namespace GeneticAlgorithms

type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      MutationRate: float
      OnGeneration: Chromosome<'Gene> -> int -> unit }
