using SharedKernel;

namespace Platform.Application.Abstractions;

/// <summary>The tenant whose records a principal may see. The composition root resolves it into the dispatch context.</summary>
[Internal]
public readonly record struct TenantId(Guid Value);
