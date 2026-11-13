using System.Reflection;
using System.Text.RegularExpressions;
using Platform.Application.Abstractions;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed partial class OperationTests
{
    private static List<Type> Messages => TypesOf(Kinds.Application, Kinds.Contracts).Where(DomainTests.IsMessage).ToList();

    [Fact]
    public void Every_message_declares_a_valid_operation()
    {
        var seen = new Dictionary<string, Type>();
        var violations = new List<string>();

        foreach (var message in Messages)
        {
            if (!typeof(IDeclaresOperation).IsAssignableFrom(message))
            {
                violations.Add($"{message.Name} implements Mediator's message interfaces directly; use ICommand or IQuery from Platform.Application");
                continue;
            }

            Operation operation;
            try
            {
                operation = (Operation)message.GetProperty(nameof(IDeclaresOperation.Operation), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
            }
            catch (TargetInvocationException e)
            {
                violations.Add($"{message.Name}'s operation is invalid: {e.InnerException?.Message}");
                continue;
            }

            var kind = Implements(message, typeof(Mediator.IBaseCommand)) ? OperationKind.Command : OperationKind.Query;
            if (operation.Kind != kind)
            {
                violations.Add($"{message.Name} is a {kind} but declares a {operation.Kind}");
            }

            var module = Owner(message)!.Module!.ToLowerInvariant();
            if (!OperationId().IsMatch(operation.Id) || !operation.Id.StartsWith(module + ".", StringComparison.Ordinal))
            {
                violations.Add($"{message.Name}'s operation id '{operation.Id}' is not '{module}.<operation>' in kebab case");
            }

            if (!seen.TryAdd(operation.Id, message))
            {
                violations.Add($"{message.Name} and {seen[operation.Id].Name} share the operation id '{operation.Id}'");
            }
        }

        violations.ShouldHaveNoViolations("Every message declares, once, its operation: id, policy, idempotency, transaction, and rate limit.");
    }

    [Fact]
    public void Commands_return_at_most_an_id_an_outcome_and_a_version()
    {
        Messages
            .Select(m => (Message: m, Response: m.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault()))
            .Where(x => x.Response is not null && !IsAllowedCommandResponse(x.Response))
            .Select(x => $"{x.Message.Name} returns {x.Response!.Name}")
            .ShouldHaveNoViolations("A command returns at most an id, an outcome, and the new version; a read is a query.");
    }

    private static bool IsAllowedCommandResponse(Type type) =>
        IsScalar(type)
        || (type.GetProperties(BindingFlags.Public | BindingFlags.Instance) is { Length: > 0 } properties && properties.All(p => IsScalar(p.PropertyType)));

    /// <summary>An id, an outcome, or a version.</summary>
    private static bool IsScalar(Type type) => IsStronglyTypedId(type) || type.IsEnum || type == typeof(long);

    [GeneratedRegex("^[a-z][a-z0-9]*\\.[a-z][a-z0-9-]*$")]
    private static partial Regex OperationId();
}
