namespace GeneticAlgorithms

type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: System.Random -> Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      CrossoverFn: System.Random -> Chromosome<'Gene> -> Chromosome<'Gene> -> Chromosome<'Gene> * Chromosome<'Gene>
      MutationRate: float
      MutationFn: System.Random -> Chromosome<'Gene> -> Chromosome<'Gene>
      ReinsertionFn:
          System.Random -> Chromosome<'Gene> array -> Chromosome<'Gene> array -> Chromosome<'Gene> array -> Chromosome<'Gene> array
      Probe: GenerationInfo<'Gene> -> unit
      /// The single source of randomness for an entire run - every strategy function draws
      /// from this instead of System.Random.Shared, so seeding it (e.g. `System.Random(42)`)
      /// makes an otherwise-identical run fully reproducible.
      Random: System.Random }
