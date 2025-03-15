# ShellRunner

Run command line tools using CSharp.

Supports Windows and Linux (Powershell, bash, and cmd).

Example running some dotnet CLI commands:

```csharp
using ShellRunner;

CommandBuilder cb = await CommandRunner
    .UsePowershell() 
    .StartProcess()
    // print the dotnet info command
    .AddCommand("dotnet --info")
    // build a csharp project
    .AddCommand(@"cd C:\source\repos\MyProject")
    .AddCommand("dotnet build MyProject.sln -c Release", key: "build-output")
    // Print the file list
    .AddCommand(@"cd bin\Release\net8.0")
    .AddCommand("Get-ChildItem -r", key: "files")
    .RunAsync();
```

Shell Runner is really useful to create a wrapper for several other 
commands and to work with data before or after you run the commands.

After running the commands you can optionally access the output of each command 
separately so you know the result of each command specifically.

```csharp
Console.WriteLine("Outputs:");

// the outputs are organized by command 
foreach (var cmd in cb.Commands)
    foreach(var output in cmd.Output)
        Console.WriteLine("  " + output.Data);

// OR use keys declared above
var buildCmd = cb.GetCommand("build-output").Output;

Console.ForegroundColor = ConsoleColor.Blue;

foreach(var output in buildCmd)
    Console.WriteLine("  " + output.Data);
```