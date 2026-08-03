namespace GeneticAlgorithms

[<Struct>]
type Chromosome<'T> =
    { genes: 'T array
      size: int
      fitness: float
      age: int }
