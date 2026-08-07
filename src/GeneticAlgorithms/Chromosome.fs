namespace GeneticAlgorithms

[<Struct>]
type Chromosome<'T> =
    { genes: 'T array
      size: int
      fitness: float
      age: int }

    member this.Genes = this.genes
    member this.Size = this.size
    member this.Fitness = this.fitness
    member this.Age = this.age
