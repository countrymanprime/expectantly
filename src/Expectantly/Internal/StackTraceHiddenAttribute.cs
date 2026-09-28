#if NETSTANDARD2_0
namespace System.Diagnostics;

// .NET 6 and later hide frames marked with this attribute from exception stack traces, so a failure
// points at the test line instead of Expectantly's internals. .NET Framework ignores it; it exists
// here only so the netstandard2.0 build compiles.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct, Inherited = false)]
internal sealed class StackTraceHiddenAttribute : Attribute
{
}
#endif
