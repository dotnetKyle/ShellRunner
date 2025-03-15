using System.Diagnostics;

namespace ShellRunner;

public class CommandBuilder
{
    CommandBuilderOptions options;
    List<ProcessCommand> commands;
    Dictionary<string, ProcessCommand> commandMap;

    public ProcessStartInfo StartInfo { get; set; }
    public Process Process { get; set; }
    public IReadOnlyList<IProcessedCommand> Commands => commands.AsReadOnly();

    internal CommandBuilder(CommandBuilderOptions options, Process process)
    {
        Process = process;

        commands = new List<ProcessCommand>();
        this.commandMap = new Dictionary<string, ProcessCommand>();

        this.options = options;

        StartInfo = new ProcessStartInfo
        {
            FileName = this.options.File,
            Arguments = this.options.Args,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardOutput = this.options.RedirectStandardOutput,
            RedirectStandardError = this.options.RedirectStandardError, 
            RedirectStandardInput = this.options.RedirectStandardInput
        };
    }

    /// <summary>
    /// Add a command to the command list.
    /// </summary>
    /// <param name="command"></param>
    /// <exception cref="ArgumentException">When a duplicate key is added.</exception>
    public void AddCommand(ProcessCommand command)
    {
        this.commands.Add(command);

        if(this.commandMap.ContainsKey(command.Key))
            throw new ArgumentException($"Command with key \"{command.Key}\" already exists.");

        this.commandMap.Add(command.Key, command);
    }

    /// <summary>
    /// Get a specific command by key.
    /// </summary>
    /// <param name="key">The key for the command ran earlier.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">When a key isn't present in the dictionary.</exception>
    public IProcessedCommand GetCommand(string key)
    {
        try
        {
            return this.commandMap[key];
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Command with key \"{key}\" not found.");
        }
    }

    public CommandBuilder AddWorkingDirectory(string workingDirectory)
    {
        StartInfo.WorkingDirectory = workingDirectory;
        return this;
    }

    public async Task<CommandBuilder> RunAsync(CancellationToken cancellationToken = default)
    {
        foreach(var command in commands)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await command.RunCommandAsync(this.Process, cancellationToken);
        }

        return this;
    }

}
