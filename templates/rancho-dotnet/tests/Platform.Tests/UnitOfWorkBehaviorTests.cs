using System.Security.Claims;
using Platform.Application.Abstractions;
using Platform.Application.Behaviors;
using Shouldly;

namespace Platform.Tests;

/// <summary>The behavior's guard for a command that declares it does not persist. No module has one yet, so a fake stands in.</summary>
public sealed class UnitOfWorkBehaviorTests
{
    private static readonly DispatchContext Context = new(new ClaimsPrincipal(), "tester", new string('a', 32));

    [Fact]
    public async Task A_command_that_does_not_persist_fails_the_dispatch_if_it_stages_a_change()
    {
        var unit = new FakeUnitOfWork { HasChanges = true };
        var behavior = new UnitOfWorkBehavior<NotifyCommand, Result>(new FakeFactory(unit), Context, TimeProvider.System);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new NotifyCommand(Guid.NewGuid()), (_, _) => ValueTask.FromResult(Result.Success()), TestContext.Current.CancellationToken));

        unit.Committed.ShouldBe(0);
    }

    [Fact]
    public async Task A_command_that_does_not_persist_is_still_recorded_in_the_audit_trail()
    {
        var unit = new FakeUnitOfWork();
        var behavior = new UnitOfWorkBehavior<NotifyCommand, Result>(new FakeFactory(unit), Context, TimeProvider.System);
        var id = Guid.NewGuid();

        var result = await behavior.Handle(new NotifyCommand(id), (_, _) => ValueTask.FromResult(Result.Success()), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        unit.Audits.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            a => a.Outcome.ShouldBe(AuditOutcome.Succeeded),
            a => a.OperationId.ShouldBe("tests.notify"),
            a => a.RecordIds.ShouldBe([id]));
        unit.Committed.ShouldBe(1);
    }

    [Fact]
    public async Task A_failure_discards_what_the_handler_staged_before_it_is_recorded()
    {
        var unit = new FakeUnitOfWork();
        var behavior = new UnitOfWorkBehavior<RenameCommand, Result>(new FakeFactory(unit), Context, TimeProvider.System);
        var problem = new Problem(new ProblemType("/problems/tests/rejected", 409, "Rejected.", UserFacing: true));

        // The handler stages a change, then fails: nothing it staged may reach the commit that records the failure.
        var result = await behavior.Handle(
            new RenameCommand(Guid.NewGuid()),
            (_, _) =>
            {
                unit.HasChanges = true;
                return ValueTask.FromResult(Result.Failure(problem));
            },
            TestContext.Current.CancellationToken);

        result.Problem.ShouldBe(problem);
        unit.Committed.ShouldBe(1);
        unit.ChangesAtCommit.ShouldBe([false]);
        unit.Audits.ShouldHaveSingleItem().Outcome.ShouldBe(AuditOutcome.Failed);
    }

    public readonly record struct SubjectId(Guid Value);

    public sealed record RenameCommand(SubjectId Subject) : ICommand
    {
        public RenameCommand(Guid subject)
            : this(new SubjectId(subject))
        {
        }

        public static Operation Operation { get; } =
            Operation.Command("tests.rename", "tests.write", IdempotencyMechanism.Natural);
    }

    public sealed record NotifyCommand(SubjectId Subject) : ICommand
    {
        public NotifyCommand(Guid subject)
            : this(new SubjectId(subject))
        {
        }

        public static Operation Operation { get; } =
            Operation.Command("tests.notify", "tests.write", IdempotencyMechanism.ClientSuppliedId, TransactionRequirement.None);
    }

    private sealed class FakeFactory(IUnitOfWork unit) : IUnitOfWorkFactory
    {
        public IUnitOfWork For(Type messageType) => unit;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool HasChanges { get; set; }

        public List<AuditEntry> Audits { get; } = [];

        public int Committed { get; private set; }

        /// <summary>Whether a handler's staged change was still there at each commit.</summary>
        public List<bool> ChangesAtCommit { get; } = [];

        public Task<RecordedResult?> FindAttempt(AttemptIdentity attempt, CancellationToken cancellationToken) => Task.FromResult<RecordedResult?>(null);

        public void StageAttempt(AttemptIdentity attempt, RecordedResult result)
        {
        }

        public void StageAudit(AuditEntry entry) => Audits.Add(entry);

        public Task<CommitOutcome> Commit(CancellationToken cancellationToken)
        {
            Committed++;
            ChangesAtCommit.Add(HasChanges);
            return Task.FromResult(CommitOutcome.Committed);
        }

        public void Discard() => HasChanges = false;
    }
}
