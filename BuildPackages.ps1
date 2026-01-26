param(
    [Parameter(Mandatory=$true)]
    [string]$releaseNotes,

    [Parameter(Mandatory=$true)]
    [string]$version
)

$ErrorActionPreference = "Stop"

function Quiet-Remove-Folder
{
    param ([string]$folder)
    if (test-path $folder)
    {
        remove-item -Path $folder -Recurse
    }
    if (test-path $folder)
    {
        Write-Error "Folder not removed: $folder"
    }
}

function Get-DirectoryPackagesVersion
{
    param([string]$id)
    $fullPath = Resolve-Path ".\Directory.Packages.props"
    $xml = New-Object -TypeName System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load($fullPath)
    $node = $xml.SelectNodes("//PackageVersion[@Include='$id']")
    $result = $node.GetAttribute("Version")
    return $result
}

function Update-Nuspec
{
    param([string]$version,[string]$name)
    $fullPath = Resolve-Path ".\$name\$name.nuspec"

    $xml = New-Object -TypeName System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load($fullPath)
    [System.Xml.XmlNamespaceManager]$ns = $xml.NameTable
    $ns.AddNamespace("nuspec", $xml.DocumentElement.NamespaceURI)

    $xml.package.metadata.version = $version
    $xml.package.metadata.releaseNotes = ($version + ' ' + $releaseNotes)

    $dependencyNodes = $xml.SelectNodes("//nuspec:dependency",$ns)
    foreach($node in $dependencyNodes)
    {
        $id = $node.GetAttribute("id")
        if($id.StartsWith("PeachtreeBus.")){
            $node.SetAttribute('version', $version)
        }
        else
        {
            $packageVersion = Get-DirectoryPackagesVersion $id
            $node.SetAttribute('version', $packageVersion)
        }
    }

    $xml.Save($fullPath)
}

function Update-DirectoryBuildProps
{
    $fullPath = Resolve-Path ".\Directory.Build.props"
    $xml = New-Object -TypeName System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load($fullPath)

    $xml.Project.PropertyGroup.Version = $version
    $xml.Project.PropertyGroup.FileVersion = $version
    $xml.Project.PropertyGroup.AssemblyVersion = ($version +".0")

    $xml.Save($fullPath)
}

function Pack-Project
{
    param([string]$name)
    dotnet pack .\$name\$name.csproj -p:NuspecFile=$name.nuspec -o .\Packages --configuration Release
}

Quiet-Remove-Folder .\PeachtreeBus.MessageInterfaces\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.MessageInterfaces\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.Abstractions\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.Abstractions\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.Core\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.Core\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.SimpleInjector\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.SimpleInjector\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.MicrosoftDependencyInjection\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.MicrosoftDependencyInjection\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.EntityFrameworkCore\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.EntityFrameworkCore\bin\Release

Update-DirectoryBuildProps

Update-Nuspec $version "PeachtreeBus.MessageInterfaces"
Update-Nuspec $version "PeachtreeBus.Abstractions"
Update-Nuspec $version "PeachtreeBus.Core"
Update-Nuspec $version "PeachtreeBus.SimpleInjector"
Update-Nuspec $version "PeachtreeBus.MicrosoftDependencyInjection"
Update-Nuspec $version "PeachtreeBus.EntityFrameworkCore"

dotnet restore PeachtreeBus.sln

dotnet build PeachtreeBus.sln -p Configuration=Nuget

Pack-Project "PeachtreeBus.MessageInterfaces"
Pack-Project "PeachtreeBus.Abstractions"
Pack-Project "PeachtreeBus.Core"
Pack-Project "PeachtreeBus.SimpleInjector"
Pack-Project "PeachtreeBus.EntityFrameworkCore"
Pack-Project "PeachtreeBus.MicrosoftDependencyInjection"