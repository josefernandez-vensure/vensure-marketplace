using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Abstractions;

namespace Api.Endpoints;

/// <summary>
/// The result translator: success to 200, 201 or 204; every failure to problem details of its declared type.
/// Its results are typed, so the OpenAPI document shows each endpoint's Response DTO.
/// </summary>
public static class ResultHttp
{
    public static Results<Ok<T>, ProblemHttpResult> ToOk<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : ToProblem(result.Problem!);

    public static Results<Ok<TResponse>, ProblemHttpResult> ToOk<T, TResponse>(this Result<T> result, Func<T, TResponse> map) =>
        result.IsSuccess ? TypedResults.Ok(map(result.Value)) : ToProblem(result.Problem!);

    public static Results<Created, ProblemHttpResult> ToCreated<T>(this Result<T> result, Func<T, string> location) =>
        result.IsSuccess ? TypedResults.Created(location(result.Value)) : ToProblem(result.Problem!);

    public static Results<NoContent, ProblemHttpResult> ToNoContent(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : ToProblem(result.Problem!);

    public static ProblemHttpResult ToProblem(Problem problem)
    {
        var details = problem.Errors is { Count: > 0 } errors
            ? new HttpValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value))
            : new ProblemDetails();
        details.Type = problem.Type.Uri;
        details.Status = problem.Type.Status;
        details.Title = problem.Type.Title;
        details.Detail = problem.Detail;
        return TypedResults.Problem(details);
    }
}
