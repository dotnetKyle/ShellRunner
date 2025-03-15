using ShellRunner;
using System.Reflection;

try
{
    var execAssemblyLocation = Assembly.GetExecutingAssembly().Location;
    var directory = Path.GetDirectoryName(execAssemblyLocation);

    if (directory is null)
        throw new Exception("Directory null exception");


    if(args.Length == 0)
    {
        Console.Error.WriteLine("Provide an argument: 'bash', 'powershell', or 'cmd'");
        return;
    }

    CommandBuilder cb;

    if (args[0] == "bash")
    {
        cb = CommandRunner
            .UseBash()
            .StartProcess();
    }
    else if (args[0] == "powershell")
    {
        cb = CommandRunner
            .UsePowershell()
            .StartProcess();
    }
    else if (args[0] == "cmd")
    {
        cb = CommandRunner
            .UseWindowsCommandShell()
            .StartProcess();
    }
    else
    {
        Console.WriteLine("Please choose bash powershell or cmd.");
        return;
    }

    await cb.AddCommand("echo off")
        .AddCommand("dotnet --info")
        .AddCommand("echo foo")
        .AddCommand("echo bar")
        .RunAsync();

    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Outputs:");
    foreach (var cmd in cb.Commands)
        foreach(var output in cmd.Output)
            Console.WriteLine("  " + output.Data);

    Console.ResetColor();


    if(args[0] == "powershell")
    {
        var cb2 = await CommandRunner
            .UsePowershell()
            .StartProcess()
            .AddWorkingDirectory(directory)
            // show what version of dotnet is loaded
            .AddCommand("dotnet --info")
            // cd into the lbirary directory
            .AddCommand("cd ../../../../MyFakeLibrary")
            // build 
            .AddCommand("dotnet build MyFakeLibrary.csproj -c Release", key: "build-output")
            .AddCommand("cd bin/Release/netstandard2.0")
            //.AddCommand("echo $myVar")
            // print artifacts
            .AddCommand("Get-Childitem", key: "get-child-item")
            // load built project into process and run one of the methods
            .AddCommand("Add-Type -Path .\\MyFakeLibrary.dll")
            .AddCommand("$obj = new-object MyFakeLibrary.TestClass")
            .AddCommand("$obj.TestLibrary('test test')", key: "test-library-output")
            .RunAsync();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Outputs:");
        Console.ResetColor();

        var buildCmd = cb2.GetCommand("build-output");
        foreach(Output output in buildCmd.Output)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            if (output.IsError)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("  " + output.Data);
        }
        Console.ResetColor();


        var listBuildFilesCmd = cb2.GetCommand("get-child-item");
        foreach (Output output in listBuildFilesCmd.Output)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (output.IsError)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("  " + output.Data);
        }
        Console.ResetColor();


        var testLibraryOutputCmd = cb2.GetCommand("test-library-output");
        foreach (Output output in testLibraryOutputCmd.Output)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (output.IsError)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("  " + output.Data);
        }
        Console.ResetColor();

    }

}
catch (Exception ex)
{
    Console.WriteLine("Unhandled error in TestApp.");
    Console.WriteLine(ex);
}