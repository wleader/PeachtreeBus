Remove-Item -Path .\PeachtreeBus\bin\Release -Recurse
Remove-Item -Path .\PeachtreeBus.SimpleInjector\bin\Release -Recurse
.\nuget.exe restore PeachtreeBus.sln

#this throws erros on the .sqlproj projects, but the other stuff does build. 
dotnet build PeachtreeBus.sln -p Configuration=Nuget

.\nuget.exe pack PeachtreeBus.nuspec -OutputDirectory .\Packages -Properties Configuration=Release
.\nuget.exe pack PeachtreeBus.SimpleInjector.nuspec -OutputDirectory .\Packages -Properties Configuration=Release