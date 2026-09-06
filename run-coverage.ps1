$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$results = Join-Path $root "TestResults"
$report = Join-Path $root "CoverageReport"
$settings = Join-Path $root "tests/coverlet.runsettings"

Remove-Item -Recurse -Force $results, $report -ErrorAction SilentlyContinue

$projects = @(
    "tests/UnitTests/UnitTests.csproj",
    "tests/IntegrationTests/IntegrationTests.csproj"
)

foreach ($project in $projects) {
    dotnet test (Join-Path $root $project) `
        --collect:"XPlat Code Coverage" `
        --settings $settings `
        --results-directory $results
}

if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

reportgenerator `
    "-reports:$results/**/coverage.cobertura.xml" `
    "-targetdir:$report" `
    "-reporttypes:Html;TextSummary" `
    "-assemblyfilters:+Pinkterest.*;-Pinkterest.Benchmarks;-Pinkterest.*Tests" `
    "-classfilters:-*.Migrations.*"

Write-Host ""
Get-Content (Join-Path $report "Summary.txt")
Write-Host ""
Write-Host "HTML report: $report/index.html"
