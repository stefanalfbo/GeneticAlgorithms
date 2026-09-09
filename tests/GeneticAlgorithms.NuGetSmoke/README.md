# GeneticAlgorithms NuGet Smoke Test

Verifies that the `GeneticAlgorithms` package installs and runs correctly when consumed the way a real user would: through a `PackageReference` resolved by version, not a `ProjectReference` to local source (which is what `GeneticAlgorithms.CSharpSmoke` uses instead).

It is deliberately excluded from `genetic-algorithms.sln` so that an ordinary `dotnet build`/`dotnet test` of the solution doesn't require a package of some specific version to already be resolvable.

## Running in CI

`.github/workflows/ci.yml` runs this smoke test on every push and pull request, against a package built from the commit under test - not an already-published version. The workflow:

1. Packs `src/GeneticAlgorithms` with `dotnet pack -p:PackageVersion=0.0.0-ci.<run id>` into a local `artifacts/nuget` folder. The run id keeps the version unique per run, so a stale cached package can never shadow the candidate.
2. Restores this project with `--source artifacts/nuget --source https://api.nuget.org/v3/index.json` - the local folder for the candidate package itself, nuget.org for its ordinary dependencies (e.g. `FSharp.Core`).
3. Runs the smoke test with `-p:GeneticAlgorithmsVersion=0.0.0-ci.<run id>` matching the packed version.

This means CI catches an API mismatch between the package's public surface and this project the same way it would catch one in any other example - the whole point of a smoke test - without ever depending on nuget.org already having the commit under test.

## Running Locally

To check a specific **published** version (for example, after a release, to confirm nuget.org has fully indexed it):

```powershell
dotnet run --project tests/GeneticAlgorithms.NuGetSmoke -p:GeneticAlgorithmsVersion=1.0.0
```

Note that a freshly published package can sit under "Unlisted" on nuget.org for a few minutes while it goes through validation and indexing, so restoring immediately after a publish may fail until that completes.

To check an unpublished, locally packed candidate instead - the same flow CI runs:

```powershell
dotnet pack src/GeneticAlgorithms -c Release -p:PackageVersion=0.0.0-local -o artifacts/nuget
dotnet restore tests/GeneticAlgorithms.NuGetSmoke -p:GeneticAlgorithmsVersion=0.0.0-local --source artifacts/nuget --source https://api.nuget.org/v3/index.json
dotnet run --project tests/GeneticAlgorithms.NuGetSmoke --configuration Release --no-restore -p:GeneticAlgorithmsVersion=0.0.0-local
```

The project runs a trivial one-gene genetic algorithm and throws if the result isn't the expected fitness, so a non-zero exit code means the package is broken.
