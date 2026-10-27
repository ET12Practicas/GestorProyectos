namespace Aplicacion.Comun;

public sealed record Error(string Codigo, string Mensaje)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error NotFound(string mensaje = "Recurso no encontrado.") => new("NotFound", mensaje);
    public static Error Validation(string mensaje = "Error de validación.") => new("Validation", mensaje);
    public static Error Conflict(string mensaje = "Conflicto con el estado actual.") => new("Conflict", mensaje);
    public static Error Unauthorized(string mensaje = "No autorizado.") => new("Unauthorized", mensaje);
    public static Error Forbidden(string mensaje = "Acceso denegado.") => new("Forbidden", mensaje);
    public static Error Failure(string mensaje = "Operación fallida.") => new("Failure", mensaje);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Un resultado exitoso no puede tener un error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Un resultado fallido debe contener un error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(Error error) => new(error);
}
