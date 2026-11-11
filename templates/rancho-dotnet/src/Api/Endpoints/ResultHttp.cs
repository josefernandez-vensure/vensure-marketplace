using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Abstractions;
using HttpResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints;

/// <summary>The result translator: success to 200, 201 or 204; every failure to problem details of its declared type.</summary>
public static class ResultHttp
{
    public static HttpResult ToOk<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : ToProblem(result.Problem!);

    public static HttpResult ToOk<T, TResponse>(this Result<T> result, Func<T, TResponse> map) =>
        result.IsSuccess ? TypedResults.Ok(map(result.Value)) : ToProblem(result.Problem!);

    public static HttpResult ToCreated<T>(this Result<T> result, Func<T, string> location) =>
        result.IsSuccess ? TypedResults.Created(location(result.Value)) : ToProblem(result.Problem!);

    public static HttpResult ToNoContent(this Result result) =>
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
