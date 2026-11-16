namespace SharedKernel;

/// <summary>
/// Marks a domain service: an operation of the domain that belongs to no single aggregate, such as one over several
/// aggregates of its module. It loads them through their repositories, applies the rule, and stages them; the pipeline
/// commits them together. A handler calls it, so the rule stays in the domain.
/// </summary>
public interface IDomainService;
