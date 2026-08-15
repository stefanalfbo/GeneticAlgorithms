namespace GeneticAlgorithms

type Options<'Gene> =
    { population_size: int
      selection_rate: float
      selection_fn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array }

    member this.PopulationSize = this.population_size
    member this.SelectionRate = this.selection_rate
