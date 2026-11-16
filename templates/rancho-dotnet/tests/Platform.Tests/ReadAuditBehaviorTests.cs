using System.Security.Claims;
using Platform.Application.Abstractions;
using Platform.Application.Behaviors;
using SharedKernel;
using Shouldly;

namespace Platform.Tests;

/// <summary>
/// Core §4: a read of Restricted data, and by default of Confidential, is audited with the ids it named and returned. The template's own
/// queries return neither, so these stand-ins prove the behavior.
/// </summary>
public sealed class ReadAuditBehaviorTests
{
    private static readonly DispatchContext Context = new(new ClaimsPrincipal(), "reader", new string('c', 32), tenant: null);

    [Fact]
    public async Task A_read_of_restricted_data_is_recorded_with_the_ids_it_named_and_returned()
    {
        var unit = new RecordingUnitOfWork();
        var named = Guid.NewGuid();
        var returned = Guid.NewGuid();
        var behavior = new ReadAuditBehavior<ReadRestricted, Result<RestrictedView>>(new Factory(unit), Context, TimeProvider.System);

        var result = await behavior.Handle(new ReadRestricted(new SubjectId(named)),
            (_, _) => ValueTask.FromResult(Result<RestrictedView>.Success(new RestrictedView(new SubjectId(returned), "123-45-6789"))),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        unit.Audits.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            a => a.OperationId.ShouldBe("tests.read-restricted"),
            a => a.Outcome.ShouldBe(AuditOutcome.Succeeded),
            a => a.RecordIds.ShouldBe([named, returned]));
        unit.Committed.ShouldBe(1);
    }

    [Fact]
    public async Task A_read_of_confidential_data_is_recorded_by_default_down_to_each_item_of_a_page()
    {
        var unit = new RecordingUnitOfWork();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var behavior = new ReadAuditBehavior<ListConfidential, Result<Page<ConfidentialView>>>(new Factory(unit), Context, TimeProvider.System);

        await behavior.Handle(new ListConfidential(),
            (_, _) => ValueTask.FromResult(Result<Page<ConfidentialView>>.Success(new Page<ConfidentialView>([new(new SubjectId(first), "a"), new(new SubjectId(second), "b")], null))),
            TestContext.Current.CancellationToken);

        unit.Audits.ShouldHaveSingleItem().RecordIds.ShouldBe([first, second]);
    }

    [Fact]
    public async Task A_read_of_internal_data_is_not_recorded()
    {
        var unit = new RecordingUnitOfWork();
        var behavior = new ReadAuditBehavior<ReadInternal, Result<InternalView>>(new Factory(unit), Context, TimeProvider.System);

        await behavior.Handle(new ReadInternal(), (_, _) => ValueTask.FromResult(Result<InternalView>.Success(new InternalView("x"))), TestContext.Current.CancellationToken);

        unit.Audits.ShouldBeEmpty();
        unit.Committed.ShouldBe(0);
    }

    [Internal]
    public readonly record struct SubjectId(Guid Value);

    public sealed record RestrictedView(SubjectId Id, [Restricted] string TaxNumber);

    public sealed record ConfidentialView(SubjectId Id, [Confidential] string Email);

    public sealed record InternalView([Internal] string Name);

    public sealed record ReadRestricted(SubjectId Subject) : IQuery<RestrictedView>
    {
        public static Operation Operation { get; } = Operation.Query("tests.read-restricted", "tests.read");
    }

    public sealed record ListConfidential : IQuery<Page<ConfidentialView>>
    {
        public static Operation Operation { get; } = Operation.Query("tests.list-confidential", "tests.read");
    }

    public sealed record ReadInternal : IQuery<InternalView>
    {
        public static Operation Operation { get; } = Operation.Query("tests.read-internal", "tests.read");
    }

    private sealed class Factory(IUnitOfWork unit) : IUnitOfWorkFactory
    {
        public IUnitOfWork For(Type messageType) => unit;
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public bool HasChanges => false;

        public IReadOnlyList<long> LoadedVersions => [];

        public bool ChangesLoadedAggregates => false;

        public List<AuditEntry> Audits { get; } = [];

        public int Committed { get; private set; }

        public Task<RecordedResult?> FindAttempt(AttemptIdentity attempt, CancellationToken cancellationToken) => Task.FromResult<RecordedResult?>(null);

        public void StageAttempt(AttemptIdentity attempt, RecordedResult result)
        {
        }

        public void StageAudit(AuditEntry entry) => Audits.Add(entry);

        public Task<CommitOutcome> Commit(CancellationToken cancellationToken)
        {
            Committed++;
            return Task.FromResult(CommitOutcome.Committed);
        }

        public void Discard() => Audits.Clear();
    }
}
