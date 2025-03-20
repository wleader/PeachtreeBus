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

Quiet-Remove-Folder .\PeachtreeBus\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.SimpleInjector\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.EntityFrameworkCore\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.Abstractions\bin\Release

.\nuget.exe restore PeachtreeBus.sln

#this throws erros on the .sqlproj projects, but the other stuff does build. 
dotnet build PeachtreeBus.sln -p Configuration=Nuget

.\nuget.exe pack PeachtreeBus.Abstractions.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.SimpleInjector.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.EntityFrameworkCore.nuspec -OutputDirectory .\Packages -Properties Configuration=Release