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

Quiet-Remove-Folder .\PeachtreeBus.Abstractions\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.Abstractions\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.Core\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.Core\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.SimpleInjector\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.SimpleInjector\bin\Release
Quiet-Remove-Folder .\PeachtreeBus.EntityFrameworkCore\bin\Debug
Quiet-Remove-Folder .\PeachtreeBus.EntityFrameworkCore\bin\Release

.\nuget.exe restore PeachtreeBus.sln

#this throws erros on the .sqlproj projects, but the other stuff does build. 
dotnet build PeachtreeBus.sln -p Configuration=Nuget

.\nuget.exe pack PeachtreeBus.Abstractions.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.Core.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.SimpleInjector.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.EntityFrameworkCore.nuspec -OutputDirectory .\Packages -Properties Configuration=Release