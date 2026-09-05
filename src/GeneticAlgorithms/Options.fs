namespace GeneticAlgorithms

type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      CrossoverFn: Chromosome<'Gene> -> Chromosome<'Gene> -> Chromosome<'Gene> * Chromosome<'Gene>
      MutationRate: float
      MutationFn: Chromosome<'Gene> -> Chromosome<'Gene>
      ReinsertionFn: Chromosome<'Gene> array -> Chromosome<'Gene> array -> Chromosome<'Gene> array -> Chromosome<'Gene> array
      Probe: GenerationInfo<'Gene> -> unit }
