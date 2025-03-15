using System.Diagnostics;

namespace ShellRunner;

public static class CommandRunner
{
    public static CommandBuilderOptions UsePowershell()
    {
        var options = new CommandBuilderOptions(
            ShellType.Powershell, 
            "powershell",
            "")
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true, 
        };
        return options;
    }
    public static CommandBuilderOptions UseBash()
    {
        var options = new CommandBuilderOptions(
            ShellType.Bash,
            "/bin/bash",
            "-v")
        {
            RedirectStandardInput = true, 
            RedirectStandardError = true, 
            RedirectStandardOutput = true
        };
        return options;
    }
    public static CommandBuilderOptions UseWindowsCommandShell()
    {
        var options = new CommandBuilderOptions(
            ShellType.WindowsCommandShell,
            "cmd",
            "/k")
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return options;
    }

    public static CommandBuilderOptions AddWorkingDirectory(
        this CommandBuilderOptions options, 
        string workingDirectory)
    {
        options = options with 
        { 
            WorkingDirectory = workingDirectory 
        };

        return options;
    }

    public static CommandBuilder StartProcess(this CommandBuilderOptions options)
    {
        var proc = new Process();

        proc.StartInfo.FileName = options.File;
        proc.StartInfo.Arguments = options.Args;
        proc.StartInfo.RedirectStandardOutput = options.RedirectStandardOutput;
        proc.StartInfo.RedirectStandardError = options.RedirectStandardError;
        proc.StartInfo.RedirectStandardInput = options.RedirectStandardInput;
        
        if(options.WorkingDirectory is not null)
            proc.StartInfo.WorkingDirectory = options.WorkingDirectory;

        proc.Start();

        var builder = new CommandBuilder(options, proc);

        return builder;
    }

    public static CommandBuilder AddCommand(this CommandBuilder builder, string command)
    {
        builder.AddCommand(new ProcessCommand(command, key:Guid.NewGuid().ToString()));
        return builder;
    }
    public static CommandBuilder AddCommand(this CommandBuilder builder, string command, string key)
    {
        builder.AddCommand(new ProcessCommand(command, key: key));
        return builder;
    }
}