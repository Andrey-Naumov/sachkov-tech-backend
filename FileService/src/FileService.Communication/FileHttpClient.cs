using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using SachkovTech.Framework.Http;
using SharedKernel;

namespace FileService.Communication;

/// <summary>
/// Клиент для взаимодействия с файловыми эндпоинтами.
/// </summary>
public class FileHttpClient(HttpClient httpClient) : IFileService
{
    public async Task<Result<StartMultipartUploadResponse, ErrorList>> StartMultipartUpload(
        StartMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/files/multipart/start", request, cancellationToken);
        return await response.HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);
    }

    public async Task<Result<CompleteMultipartUploadResponse, ErrorList>> CompleteMultipartUpload(
        CompleteMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/files/multipart/end", request, cancellationToken);
        return await response.HandleResponseAsync<CompleteMultipartUploadResponse>(cancellationToken);
    }

    public async Task<Result<GetChunkUploadUrlResponse, ErrorList>> GetChunkUploadUrl(
        GetChunkUploadUrlRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/files/multipart/url", request, cancellationToken);
        return await response.HandleResponseAsync<GetChunkUploadUrlResponse>(cancellationToken);
    }

    public async Task<Result<GetDownloadUrlResponse, ErrorList>> GetDownloadUrl(
        GetDownloadUrlRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/files/url", request, cancellationToken);
        return await response.HandleResponseAsync<GetDownloadUrlResponse>(cancellationToken);
    }

    public async Task<Result<GetDownloadUrlsResponse, ErrorList>> GetDownloadUrls(
        GetDownloadUrlsRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/files/urls", request, cancellationToken);
        return await response.HandleResponseAsync<GetDownloadUrlsResponse>(cancellationToken);
    }

    public async Task<Result<GetHlsPlaylistUrlResponse, ErrorList>> GetHlsPlaylistUrl(
        Guid videoId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/files/hls/{videoId}/playlist", cancellationToken);
        return await response.HandleResponseAsync<GetHlsPlaylistUrlResponse>(cancellationToken);
    }
}