using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ShellRunner;

/// <summary>
/// A Process Command.
/// </summary>
public class ProcessCommand : IProcessedCommand
{
    readonly CompletionToken completionToken;
    readonly TaskCompletionSource outputCompletion;
    readonly List<Output> output;

    public ProcessCommand(string command, string key)
    {
        this.Command = command;
        this.completionToken = new CompletionToken();
        this.outputCompletion = new TaskCompletionSource();
        this.output = new List<Output>();
        this.Key = key;
    }

    public string Command { get; }
    public string Key { get; }
    public IReadOnlyList<Output> Output => output.AsReadOnly();
    public bool IsError { get; private set; }

    /// <summary>
    /// Run this command.
    /// </summary>
    /// <param name="process">The process to run the command on</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RunCommandAsync(Process process, CancellationToken cancellationToken)
    {
        try
        {
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await RunAsync(process, cancellationToken);

            // send a signal to look for in the output so you know the command is finished
            this.completionToken.SendCompletionSignal(process);

            await Task.WhenAny(
                this.outputCompletion.Task, 
                OrCancelWhen(cancellationToken)
            );

            process.CancelOutputRead();
            process.CancelErrorRead();
        }
        catch(Exception ex)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error while running command.");
            Console.WriteLine(ex);
            Console.ForegroundColor = fg;
        }
        finally
        {
            // cleanup events
            process.OutputDataReceived -= Process_OutputDataReceived;
            process.ErrorDataReceived -= Process_ErrorDataReceived;
        }
    }

    // this event is ran when this command receives output data from STD_OUT
    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if(isValidData(e.Data))
        {
            output.Add(new Output(IsError: false, e.Data));
        }
    }

    // this event is ran when this command receives output data from STD_ERROR
    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (isValidData(e.Data))
        {
            output.Add(new Output(IsError: true, e.Data));
        }
        IsError = true;
    }

    
    bool isValidData([NotNullWhen(true)] string? output)
    {
        if(output is null)
        {
            return false;
        }

        if (output == this.completionToken)
        {
            // this is the completion signal so stop waiting for output
            outputCompletion.TrySetResult();
            return false;
        }
        
        if (output.EndsWith(this.completionToken.Value))
        {
            // return false so we don't save the output for the completion token
            return false;
        }

        // valid data so return true
        return true;
    }

    /// <summary>
    /// Run this task.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task RunAsync(Process process, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        process.StandardInput.WriteLine(Command);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a task that watches the cancellation token for cancellation.
    /// </summary>
    static Task OrCancelWhen(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        cancellationToken.Register(() => tcs.TrySetResult());
        return tcs.Task;
    }
}
