using System.Diagnostics;

namespace ShellRunner;

/// <summary>
/// Represents a signal that is sent to the STD_IN stream after a command 
/// is ran that when detected in the STD_OUT stream notifies the system 
/// when the command has finished running.
/// </summary>
struct CompletionToken
{
    public CompletionToken()
    {
        this.Value = Guid.NewGuid().ToString();
    }

    public string Value { get; }

    public static bool operator ==(CompletionToken a, CompletionToken b) =>
        a.Value == b.Value;
    public static bool operator !=(CompletionToken a, CompletionToken b) =>
        a.Value != b.Value;
    
    public static bool operator ==(CompletionToken a, object b)
    {
        if (b is CompletionToken ct)
            return a.Value == ct.Value;
        
        if(b is string str)
            return a.Value == str;

        return false;
    }
    public static bool operator !=(CompletionToken a, object b)
    {
        if (b is CompletionToken ct)
            return a.Value != ct.Value;
        
        if (b is string str)
            return a.Value != str;

        return true;
    }
    public static bool operator ==(object b, CompletionToken a)
    {
        if (b is CompletionToken ct)
            return a.Value == ct.Value;

        if (b is string str)
            return a.Value == str;

        return false;
    }
    public static bool operator !=(object b, CompletionToken a)
    {
        if (b is CompletionToken ct)
            return a.Value != ct.Value;

        if (b is string str)
            return a.Value != str;

        return true;
    }
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is CompletionToken ct)
            return ct.Value == this.Value;

        return false;
    }
    public override int GetHashCode() =>
        Value.GetHashCode();

    public void SendCompletionSignal(Process process)
    {
        process.StandardInput.WriteLine($"echo {Value}");
        process.StandardInput.Flush();
    }
}
