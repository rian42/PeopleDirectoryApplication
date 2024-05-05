# PeopleDirectoryApplication

Hi there!

This is a Code-First, entity framework project. To start, ensure that you have your connection string for your DB nearby...

1. Enter your connection string in the `appsettings.json` file.
2. Open package manager console in the project directory and run `Upgrade-Database`
3. If this succeeds, I have prepared a `Seed` class to ensure you are up and running. 
   Check `Program.cs` that both the `SeedPersonData` and  `SeedRolesAsync` methods are uncommented
4. Add the admin user credentials in `appsettings.json`
5. Open a console in the project directory and run the command `dotnet run seeddata`
6. Once that finishes you can stop the pipeline, and debug the app for testing purposes
 
