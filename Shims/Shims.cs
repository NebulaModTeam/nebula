using System;
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct,
        AllowMultiple = false, Inherited = false)]
    sealed class ExcludeFromCodeCoverageAttribute : Attribute { }
}