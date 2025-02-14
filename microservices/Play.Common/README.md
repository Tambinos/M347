From Root directory

dotnet pack .\Play.Common\src\Play.Common\ --configuration Release -p:PackageVersion=1.0.20.1  -o .\packages





Wechseln ins Verzeichniss packages

dotnet nuget push .\packages\Play.Common.1.0.20.1.nupkg --source https://git.gibb.ch/api/v4/projects/5940/packages/nuget/index.json --api-key glpat-8GxsWez9YrpVnGygyg


dotnet nuget push .\packages\Play.Catalog.Contracts.1.0.1.nupkg --source https://git.gibb.ch/api/v4/projects/5940/packages/nuget/index.json --api-key glpat-8GxsWezmYrpVnGygyg

