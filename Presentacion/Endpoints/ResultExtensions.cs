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
            return string.IsNullOrEmpty(createdUri)
                ? Results.Ok(result.Value)
                : Results.Created(createdUri, new { id = result.Value });
        }

        return MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Codigo switch
    {
        "NotFound" => Results.NotFound(new { error = error.Mensaje }),
        "Conflict" => Results.Conflict(new { error = error.Mensaje }),
        _ => Results.BadRequest(new { error = error.Mensaje })
    };
}
