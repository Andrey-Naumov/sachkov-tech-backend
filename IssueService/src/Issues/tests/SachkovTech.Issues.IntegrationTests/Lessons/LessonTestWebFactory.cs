using CSharpFunctionalExtensions;
using FileService.Communication;
using FileService.Contracts;
using FileService.Contracts.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using SachkovTech.Core.Caching;
using SharedKernel;

namespace SachkovTech.Issues.IntegrationTests.Lessons;

public class LessonTestWebFactory : IntegrationTestsWebFactory
{
    private readonly IFileService _fileServiceMock = Substitute.For<IFileService>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    private const string FILE_ID_TEST = "106c352b-98d2-4a31-8ea6-3f63c5f2d27a";

    public void SetupSuccessFileServiceMock()
    {
        var response = new CompleteMultipartUploadResponse(FILE_ID_TEST);
        _fileServiceMock
            .CompleteMultipartUpload(Arg.Any<CompleteMultipartUploadRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<CompleteMultipartUploadResponse, ErrorList>(response));

        _fileServiceMock
            .GetDownloadUrls(Arg.Any<GetDownloadUrlsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<GetDownloadUrlsResponse, ErrorList>(
                new GetDownloadUrlsResponse([new FileUrl($"testUrl/{FILE_ID_TEST}", "test")])));

        _cacheServiceMock
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())!
            .Returns(Task.FromResult<string>(null));

        _cacheServiceMock
            .SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    public void SetupSuccessFileServiceMock(IEnumerable<Guid> fileIds)
    {
        var response = new CompleteMultipartUploadResponse(FILE_ID_TEST);
        _fileServiceMock
            .CompleteMultipartUpload(Arg.Any<CompleteMultipartUploadRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<CompleteMultipartUploadResponse, ErrorList>(response));

        var urls = fileIds
            .Select(id => new FileUrl($"{id}", $"test/{id}"))
            .ToArray();
        _fileServiceMock
            .GetDownloadUrls(Arg.Any<GetDownloadUrlsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<GetDownloadUrlsResponse, ErrorList>(new GetDownloadUrlsResponse(urls)));

        _fileServiceMock
            .GetHlsPlaylistUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<GetHlsPlaylistUrlResponse, ErrorList>(new GetHlsPlaylistUrlResponse($"test/{Guid.Empty}")));

        _cacheServiceMock
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())!
            .Returns(Task.FromResult<string>(null));

        _cacheServiceMock
            .SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    public void SetupFailureFileServiceMock()
    {
        _fileServiceMock
            .CompleteMultipartUpload(Arg.Any<CompleteMultipartUploadRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CompleteMultipartUploadResponse, ErrorList>(Errors.General.Failure()));

        _fileServiceMock
            .GetDownloadUrls(Arg.Any<GetDownloadUrlsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<GetDownloadUrlsResponse, ErrorList>(Errors.General.Failure()));

        _cacheServiceMock
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())!
            .Returns(Task.FromResult<string>(null));
    }

    protected override void ConfigureDefaultServices(IServiceCollection services)
    {
        base.ConfigureDefaultServices(services);

        var fileServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IFileService));
        if (fileServiceDescriptor != null)
            services.Remove(fileServiceDescriptor);

        var minioOptions = new MinioOptions { UrlExpirationDays = 6 };

        var optionsWrapper = Options.Create(minioOptions);

        var fileServiceCachingDecorator = new FileServiceCachingDecorator(
            _fileServiceMock,
            _cacheServiceMock,
            optionsWrapper);

        services.AddScoped<IFileService, FileHttpClient>();
        services.Decorate<IFileService>(_ => fileServiceCachingDecorator);
    }
}