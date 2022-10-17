# Publishing of Flowifier ".NET Core" Client Library on NuGet.org

1. Set the new version number in project file "FlowifierClient.Library.csproj" -> [SemVer](https://semver.org/lang/de/)
2. Ensure release notes are documented
3. Do a clean build the release version of the library
4. Go in command prompt (terminal) to the folder ".\FlowifierClient.Library\bin\Release"
5. Execute the following command:
 ```console
 dotnet nuget push Flowifier.Client.<VERSION>.nupkg --api-key <NUGET-ORG-API-KEY> --source https://api.nuget.org/v3/index.json
 ```
6. Commit all changes and make a version branch if major or minor has changed or a tag if patch has changed