//==================================================================================================
// Global analyzer suppressions for Fox.RetryKit.
// Modern SDK-style projects use Version from .csproj instead of AssemblyVersion attribute.
// Retry library intentionally catches general exceptions to provide resilience.
//==================================================================================================
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1016:Mark assemblies with AssemblyVersionAttribute", Justification = "Modern SDK-style projects use <Version> in .csproj file instead of AssemblyVersion attribute.")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Retry library intentionally catches all exceptions to provide resilience and allow retry logic for any failure type.")]
