using System.Diagnostics;

namespace ShellRunner;

[DebuggerDisplay("{IsError ? \"Error: \" : \"\"}{Data}")]
public record struct Output(bool IsError, string Data);
