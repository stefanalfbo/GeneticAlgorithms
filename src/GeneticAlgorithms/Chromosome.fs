namespace GeneticAlgorithms

type Chromosome<'T> =
    { Genes: 'T array
      Fitness: float
      Age: int }

    member this.Size = this.Genes.Length
