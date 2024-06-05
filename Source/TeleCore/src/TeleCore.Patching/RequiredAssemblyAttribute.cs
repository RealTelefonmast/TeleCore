using System;
using System.Reflection;

namespace TeleCore.Patching;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = true)]
public class RequiredAssemblyAttribute : Attribute
{
    public AssemblyName Name { get; }

    public RequiredAssemblyAttribute(AssemblyName name)
    {
        Name = name;
    }
}