//==================================================================================================
// Global analyzer suppressions for Fox.RetryKit.Tests.
// Test projects do not require AssemblyVersion attribute and may catch general exceptions.
//==================================================================================================
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1016:Mark assemblies with AssemblyVersionAttribute", Justification = "Test projects do not require AssemblyVersion attribute.")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Tests intentionally catch general exceptions to verify retry behavior.")]
