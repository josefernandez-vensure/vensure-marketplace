using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using SharedKernel;
using Shouldly;

namespace Api.Tests;

/// <summary>What a log provider receives through the host's logging, redacted as the composition root configures it.</summary>
public sealed partial class RedactionTests
{
    private const string Address = "someone@example.com";
    private const string Operation = "catalog.contact-supplier";

    [Fact]
    public void A_classified_logging_parameter_is_redacted_in_the_providers_output()
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddFakeLogging()));
        var logger = factory.Services.GetRequiredService<ILogger<RedactionTests>>();

        Contacted(logger, Operation, new EmailAddress(Address));

        var record = factory.Services.GetFakeLogCollector().GetSnapshot().Single(r => r.Category == typeof(RedactionTests).FullName);
        record.Message.ShouldNotContain(Address);
        record.StructuredState.ShouldNotBeNull().ShouldNotContain(kv => kv.Value != null && kv.Value.Contains(Address, StringComparison.Ordinal));
        record.Message.ShouldContain(Operation);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} contacted {Address}")]
    private static partial void Contacted(ILogger logger, [Internal] string operation, [Confidential] EmailAddress address);

    /// <summary>Logged as a value would be by mistake: its type's own ToString hides nothing, so only the redactor can.</summary>
    [Confidential]
    private sealed record EmailAddress(string Value)
    {
        public override string ToString() => Value;
    }
}
