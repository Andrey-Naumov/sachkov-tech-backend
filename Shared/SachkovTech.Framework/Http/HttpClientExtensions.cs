using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace SachkovTech.Framework.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, ErrorList>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);
            return error?.Errors ?? Error.Failure("server.internal", "Server error").ToErrorList();
        }

        var result = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

        if (result?.Result == null)
        {
            return Error.Failure("deserialization.error", "Failed to deserialize response").ToErrorList();
        }

        if (result.IsError)
        {
            return result.Errors ?? Error.Failure("server.internal", "Server error").ToErrorList();
        }

        return result.Result;
    }
}