namespace GeneticAlgorithms

open System
open System.Collections.Generic

[<AbstractClass; Sealed>]
type GeneticAlgorithm =
    static member CreateChromosome<'Gene>(genes: 'Gene array) : Chromosome<'Gene> =
        if isNull genes then
            nullArg "genes"

        { Genes = Array.copy genes
          Fitness = 0.0
          Age = 0 }

    static member CreateOptions<'Gene>(populationSize: int) : Options<'Gene> =
        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = Selection.elite
          CrossoverFn = Crossover.singlePoint
          MutationRate = 0.05
          MutationFn = Mutation.scramble
          ReinsertionFn = Reinsertion.elitist 0.15
          Probe = Probes.noop
          Random = Random() }

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>
        ) : Options<'Gene> =
        if isNull selectionFn then
            nullArg "selectionFn"

        if isNull crossoverFn then
            nullArg "crossoverFn"

        if isNull mutationFn then
            nullArg "mutationFn"

        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = fun rng population n -> selectionFn.Invoke(rng, population, n)
          CrossoverFn = fun rng p1 p2 -> crossoverFn.Invoke(rng, p1, p2)
          MutationRate = 0.05
          MutationFn = fun rng chromosome -> mutationFn.Invoke(rng, chromosome)
          ReinsertionFn = Reinsertion.elitist 0.15
          Probe = Probes.noop
          Random = Random() }

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>
        ) : Options<'Gene> =
        if isNull selectionFn then
            nullArg "selectionFn"

        if isNull crossoverFn then
            nullArg "crossoverFn"

        if isNull mutationFn then
            nullArg "mutationFn"

        if isNull reinsertionFn then
            nullArg "reinsertionFn"

        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = fun rng population n -> selectionFn.Invoke(rng, population, n)
          CrossoverFn = fun rng p1 p2 -> crossoverFn.Invoke(rng, p1, p2)
          MutationRate = 0.05
          MutationFn = fun rng chromosome -> mutationFn.Invoke(rng, chromosome)
          ReinsertionFn = fun rng parents offspring leftover -> reinsertionFn.Invoke(rng, parents, offspring, leftover)
          Probe = Probes.noop
          Random = Random() }

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>
        ) : Options<'Gene> =
        if isNull selectionFn then
            nullArg "selectionFn"

        if isNull crossoverFn then
            nullArg "crossoverFn"

        if isNull mutationFn then
            nullArg "mutationFn"

        if isNull reinsertionFn then
            nullArg "reinsertionFn"

        if isNull probe then
            nullArg "probe"

        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = fun rng population n -> selectionFn.Invoke(rng, population, n)
          CrossoverFn = fun rng p1 p2 -> crossoverFn.Invoke(rng, p1, p2)
          MutationRate = 0.05
          MutationFn = fun rng chromosome -> mutationFn.Invoke(rng, chromosome)
          ReinsertionFn = fun rng parents offspring leftover -> reinsertionFn.Invoke(rng, parents, offspring, leftover)
          Probe = fun info -> probe.Invoke(info)
          Random = Random() }

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            mutationRate: float,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>
        ) : Options<'Gene> =
        if isNull selectionFn then
            nullArg "selectionFn"

        if isNull crossoverFn then
            nullArg "crossoverFn"

        if isNull mutationFn then
            nullArg "mutationFn"

        if isNull reinsertionFn then
            nullArg "reinsertionFn"

        if isNull probe then
            nullArg "probe"

        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = fun rng population n -> selectionFn.Invoke(rng, population, n)
          CrossoverFn = fun rng p1 p2 -> crossoverFn.Invoke(rng, p1, p2)
          MutationRate = mutationRate
          MutationFn = fun rng chromosome -> mutationFn.Invoke(rng, chromosome)
          ReinsertionFn = fun rng parents offspring leftover -> reinsertionFn.Invoke(rng, parents, offspring, leftover)
          Probe = fun info -> probe.Invoke(info)
          Random = Random() }

    /// The fullest overload, additionally exposing <c>random</c> so a run can be made
    /// reproducible: pass the same seeded <c>System.Random</c> (e.g. <c>new Random(42)</c>)
    /// across two otherwise-identical runs and every draw the algorithm makes - selection,
    /// crossover, mutation, reinsertion, and (if <c>Options.create</c>'s F# counterpart or a
    /// genotype built from this same instance is used) the initial population too - comes
    /// from the same deterministic sequence.
    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            mutationRate: float,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>,
            random: Random
        ) : Options<'Gene> =
        if isNull selectionFn then
            nullArg "selectionFn"

        if isNull crossoverFn then
            nullArg "crossoverFn"

        if isNull mutationFn then
            nullArg "mutationFn"

        if isNull reinsertionFn then
            nullArg "reinsertionFn"

        if isNull probe then
            nullArg "probe"

        if isNull random then
            nullArg "random"

        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = fun rng population n -> selectionFn.Invoke(rng, population, n)
          CrossoverFn = fun rng p1 p2 -> crossoverFn.Invoke(rng, p1, p2)
          MutationRate = mutationRate
          MutationFn = fun rng chromosome -> mutationFn.Invoke(rng, chromosome)
          ReinsertionFn = fun rng parents offspring leftover -> reinsertionFn.Invoke(rng, parents, offspring, leftover)
          Probe = fun info -> probe.Invoke(info)
          Random = random }

    static member CreateProblem<'Gene>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>
        ) : Problem<'Gene> =
        if isNull genotype then
            nullArg "genotype"

        if isNull fitnessFunction then
            nullArg "fitnessFunction"

        if isNull terminate then
            nullArg "terminate"

        { Genotype = fun rng -> genotype.Invoke rng
          FitnessFunction = fun chromosome -> fitnessFunction.Invoke chromosome
          Terminate = fun population generation temperature -> terminate.Invoke(population, generation, temperature) }

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int
        ) : Chromosome<'Gene> =
        let problem = GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)
        let options = GeneticAlgorithm.CreateOptions(populationSize)

        Genetic.run problem options

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int,
            probe: Action<GenerationInfo<'Gene>>
        ) : Chromosome<'Gene> =
        if isNull probe then
            nullArg "probe"

        let problem = GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)

        let options =
            { GeneticAlgorithm.CreateOptions populationSize with
                Probe = fun info -> probe.Invoke(info) }

        Genetic.run problem options

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            options: Options<'Gene>
        ) : Chromosome<'Gene> =
        if isNull (box options) then
            nullArg "options"

        let problem = GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)

        Genetic.run problem options

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, populationSize: int) : Chromosome<'Gene> =
        if isNull (box problem) then
            nullArg "problem"

        Genetic.run problem (GeneticAlgorithm.CreateOptions populationSize)

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, options: Options<'Gene>) : Chromosome<'Gene> =
        if isNull (box problem) then
            nullArg "problem"

        if isNull (box options) then
            nullArg "options"

        Genetic.run problem options

[<AbstractClass; Sealed>]
type Interop =
    static member CreateChromosome<'Gene>(genes: 'Gene array) : Chromosome<'Gene> =
        GeneticAlgorithm.CreateChromosome genes

    static member CreateOptions<'Gene>(populationSize: int) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions populationSize

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>
        ) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions(populationSize, selectionFn, crossoverFn, mutationFn)

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>
        ) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions(populationSize, selectionFn, crossoverFn, mutationFn, reinsertionFn)

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>
        ) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions(populationSize, selectionFn, crossoverFn, mutationFn, reinsertionFn, probe)

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            mutationRate: float,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>
        ) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions(populationSize, selectionFn, crossoverFn, mutationFn, mutationRate, reinsertionFn, probe)

    static member CreateOptions<'Gene>
        (
            populationSize: int,
            selectionFn: Func<Random, Chromosome<'Gene> array, int, Chromosome<'Gene> array>,
            crossoverFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>, Chromosome<'Gene> * Chromosome<'Gene>>,
            mutationFn: Func<Random, Chromosome<'Gene>, Chromosome<'Gene>>,
            mutationRate: float,
            reinsertionFn: Func<Random, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array, Chromosome<'Gene> array>,
            probe: Action<GenerationInfo<'Gene>>,
            random: Random
        ) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions(populationSize, selectionFn, crossoverFn, mutationFn, mutationRate, reinsertionFn, probe, random)

    static member CreateProblem<'Gene>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>
        ) : Problem<'Gene> =
        GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int
        ) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(genotype, fitnessFunction, terminate, populationSize)

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int,
            probe: Action<GenerationInfo<'Gene>>
        ) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(genotype, fitnessFunction, terminate, populationSize, probe)

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Random, Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            options: Options<'Gene>
        ) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(genotype, fitnessFunction, terminate, options)

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, populationSize: int) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(problem, populationSize)

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, options: Options<'Gene>) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(problem, options)
