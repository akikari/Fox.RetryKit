//==================================================================================================
// Global analyzer suppressions for demo project.
// Demo code intentionally catches general exceptions for demonstration purposes.
//==================================================================================================
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Demo code intentionally catches all exceptions for illustration purposes.", Scope = "member", Target = "~M:Program.<Main>$(System.String[])")]
