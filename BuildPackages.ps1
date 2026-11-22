param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [string] $ReleaseNotes = ""
)

$ErrorActionPreference = "Stop"

$outputPath = Join-Path $PSScriptRoot "Packages"
$solutionPath = Join-Path $PSScriptRoot "PeachtreeBus.Packages.slnx"
New-Item $outputPath -ItemType Directory | Out-Null

dotnet clean $solutionPath `
    --configuration Release `
    --nologo
if ($LASTEXITCODE -ne 0) {
    throw "dotnet clean failed with exit code $LASTEXITCODE"
}

dotnet build $solutionPath `
    --configuration Release `
    --nologo `
    "-p:Version=$Version" `
    "-p:PackageVersion=$Version" `
    "-p:FileVersion=$Version" `
    "-p:AssemblyVersion=$Version.0"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

dotnet pack $solutionPath `
    --configuration Release `
    --no-build `
    --no-restore `
    --output $outputPath `
    "-p:Version=$Version" `
    "-p:PackageVersion=$Version" `
    "-p:PackageReleaseNotes=$ReleaseNotes"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE"
}