dotnet publish -c Release
powershell -Command "Compress-Archive -Path bin\release\netcoreapp2.0\publish\* -DestinationPath publish\latest.zip -Force"