using Aplicacion.Comun;

namespace Presentacion.Endpoints;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return MapError(result.Error);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, string? createdUri = null)
    {
        if (result.IsSuccess)
        {
            if (string.IsNullOrEmpty(createdUri))
            {
                return Results.Ok(result.Value);
            }

            var body = result.Value is Guid or string or int
                ? (object)new { id = result.Value }
                : result.Value;

            return Results.Created(createdUri, body);
        }

        return MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Codigo switch
    {
        "NotFound" => Results.NotFound(new { error = error.Mensaje }),
        "Conflict" => Results.Conflict(new { error = error.Mensaje }),
        "Unauthorized" => Results.Json(new { error = error.Mensaje }, statusCode: StatusCodes.Status401Unauthorized),
        "Forbidden" => Results.Json(new { error = error.Mensaje }, statusCode: StatusCodes.Status403Forbidden),
        _ => Results.BadRequest(new { error = error.Mensaje })
    };
}
