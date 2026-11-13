using System.Security.Claims;
using Platform.Application.Abstractions;
using Platform.Application.Behaviors;
using Shouldly;

namespace Platform.Tests;

/// <summary>The behavior's guards no module's command can trip on purpose: a fake stands in for the unit of work.</summary>
public sealed class UnitOfWorkBehaviorTests
{
    private static readonly DispatchContext Context = new(new ClaimsPrincipal(), "tester", new string('a', 32), tenant: null);

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

    [Fact]
    public async Task A_write_to_a_loaded_aggregate_without_a_precondition_fails_the_dispatch()
    {
        var unit = new FakeUnitOfWork();
        var behavior = new UnitOfWorkBehavior<RenameCommand, Result>(new FakeFactory(unit), Context, TimeProvider.System);

        // The handler changes an aggregate it loaded, but the message carries no If-Match.
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(
                new RenameCommand(Guid.NewGuid()),
                (_, _) =>
                {
                    unit.HasChanges = true;
                    unit.ChangesLoadedAggregates = true;
                    return ValueTask.FromResult(Result.Success());
                },
                TestContext.Current.CancellationToken));

        unit.Committed.ShouldBe(0);
    }

    [Fact]
    public async Task A_reaction_that_loses_a_race_is_not_recorded_so_its_retry_runs_again()
    {
        // The reaction changes the aggregate it loaded, without If-Match, and someone else commits first.
        var unit = new FakeUnitOfWork { Outcomes = new Queue<CommitOutcome>([CommitOutcome.Conflict]) };
        var reaction = new DispatchContext(new ClaimsPrincipal(), "service:tests", new string('b', 32), tenant: null,
            idempotencyKey: Guid.NewGuid().ToString(), causation: new Causation(Guid.NewGuid(), "user"));
        var behavior = new UnitOfWorkBehavior<ReactCommand, Result>(new FakeFactory(unit), reaction, TimeProvider.System);

        var result = await behavior.Handle(
            new ReactCommand(Guid.NewGuid()),
            (_, _) =>
            {
                unit.HasChanges = true;
                unit.ChangesLoadedAggregates = true;
                return ValueTask.FromResult(Result.Success());
            },
            TestContext.Current.CancellationToken);

        result.Problem?.Type.ShouldBe(PlatformProblems.Conflict);
        unit.Attempts.ShouldBeEmpty("a lost race did nothing, so a replay under the same key must run again, not answer the conflict");
        unit.Audits.Last().ShouldSatisfyAllConditions(
            a => a.Outcome.ShouldBe(AuditOutcome.Failed),
            a => a.OnBehalfOf.ShouldBe("user"),
            a => a.CausationId.ShouldBe(reaction.Causation!.MessageId));
    }

    public readonly record struct SubjectId(Guid Value);

    public sealed record ReactCommand(SubjectId Subject) : ICommand
    {
        public ReactCommand(Guid subject)
            : this(new SubjectId(subject))
        {
        }

        public static Operation Operation { get; } =
            Operation.Command("tests.react", "tests.react", IdempotencyMechanism.Key);
    }

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

        public IReadOnlyList<long> LoadedVersions { get; set; } = [];

        public bool ChangesLoadedAggregates { get; set; }

        public List<AuditEntry> Audits { get; } = [];

        public List<RecordedResult> Attempts { get; } = [];

        /// <summary>What each commit answers, in order; once empty, every commit succeeds.</summary>
        public Queue<CommitOutcome> Outcomes { get; init; } = new();

        public int Committed { get; private set; }

        /// <summary>Whether a handler's staged change was still there at each commit.</summary>
        public List<bool> ChangesAtCommit { get; } = [];

        public Task<RecordedResult?> FindAttempt(AttemptIdentity attempt, CancellationToken cancellationToken) => Task.FromResult<RecordedResult?>(null);

        public void StageAttempt(AttemptIdentity attempt, RecordedResult result) => Attempts.Add(result);

        public void StageAudit(AuditEntry entry) => Audits.Add(entry);

        public Task<CommitOutcome> Commit(CancellationToken cancellationToken)
        {
            Committed++;
            ChangesAtCommit.Add(HasChanges);
            return Task.FromResult(Outcomes.TryDequeue(out var outcome) ? outcome : CommitOutcome.Committed);
        }

        /// <summary>Like the change tracker: everything staged since the last commit is dropped.</summary>
        public void Discard()
        {
            Attempts.Clear();
            Audits.Clear();
            HasChanges = false;
            ChangesLoadedAggregates = false;
        }
    }
}
