using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace SachkovTech.Framework.Endpoints;

public static class ResultResponse
{
    public static IResult Ok()
    {
        var envelope = Envelope.Ok();
        return Results.Ok(envelope);
    }

    public static IResult Ok<T>(T? value)
    {
        var envelope = Envelope<T>.Ok(value);
        return Results.Ok(envelope);
    }

    public static IResult BadRequest<T>(ErrorList errors)
    {
        var envelope = Envelope<T>.Error(errors);
        return Results.BadRequest(envelope);
    }

    public static IResult BadRequest(ErrorList errors)
    {
        var envelope = Envelope.Error(errors);
        return Results.BadRequest(envelope);
    }

    public static IResult NotFound<T>(ErrorList errors)
    {
        var envelope = Envelope<T>.Error(errors);
        return Results.NotFound(envelope);
    }

    public static IResult NotFound(ErrorList errors)
    {
        var envelope = Envelope.Error(errors);
        return Results.NotFound(envelope);
    }
}