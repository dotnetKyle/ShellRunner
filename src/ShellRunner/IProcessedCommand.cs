namespace ShellRunner;

/// <summary>
/// An interface for the parts of a command that are needed after the command has been run.
/// </summary>
public interface IProcessedCommand
{
    /// <summary>
    /// The original command that was ran.
    /// </summary>
    string Command { get; }
    /// <summary>
    /// The key that the command was stored under. This key is typically automatically generated but you can 
    /// provide a custom key using <c>AddCommand(command: "echo bar", key: "foo")</c>.
    /// </summary>
    string Key { get; }
    /// <summary>
    /// List of the outputs of the command.
    /// </summary>
    IReadOnlyList<Output> Output { get; }
    /// <summary>
    /// True if the command has an error.
    /// </summary>
    bool IsError { get; }
}
