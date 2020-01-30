# S100Lint
This is a variation on the UNIX Lint program. This S100Lint program analyses IHO S1xxx XML Schema's and 
validates them with the specified Feature Catalogue. As a second option it is possible to have it cross 
reference two specified XML Schema's and scans them for commonalities. If it finds one it checks the 
specified XML if the found nodes are equal. Any difference gets reported. 

S100Lint requires .NET Core 3.1

https://dotnet.microsoft.com/download/dotnet-core/3.1

Building the solution requires Visual Studio 2019 for Windows or Mac.

S100Lint usage:
S100Lint [SchemaFileName] [SchemaFileName | FeatureCatalogueFileName]
