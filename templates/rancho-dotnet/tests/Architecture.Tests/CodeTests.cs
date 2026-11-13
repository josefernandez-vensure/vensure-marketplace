using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class CodeTests
{
    [Fact]
    public void No_dynamic()
    {
        DependenciesOf(Model.Types)
            .Where(d => d.Target.FullName == "Microsoft.CSharp.RuntimeBinder.Binder")
            .Select(d => $"{d.Origin.FullName} uses dynamic")
            .ShouldHaveNoViolations("No dynamic.");
    }

    [Fact]
    public void No_static_mutable_state()
    {
        const BindingFlags statics = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        Assemblies.SelectMany(a => a.Assembly.GetTypes())
            .Where(t => !IsGeneratedOrInside(t))
            .SelectMany(t =>
                t.GetFields(statics)
                    .Where(f => !f.IsInitOnly && !f.IsLiteral && !f.IsDefined(typeof(CompilerGeneratedAttribute)))
                    .Select(f => $"{t.FullName}.{f.Name} is a static mutable field")
                .Concat(t.GetProperties(statics)
                    .Where(p => p.SetMethod is not null)
                    .Select(p => $"{t.FullName}.{p.Name} is a static property with a setter")))
            .ShouldHaveNoViolations("No static mutable state; state lives in the container or the database.");
    }

    private static bool IsGeneratedOrInside(Type type)
    {
        for (var t = type; t is not null; t = t.DeclaringType)
        {
            if (t.IsDefined(typeof(CompilerGeneratedAttribute), false) || t.IsDefined(typeof(GeneratedCodeAttribute), false) || t.Name.StartsWith('<'))
            {
                return true;
            }
        }

        return false;
    }
}
