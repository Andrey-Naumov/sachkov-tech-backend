using FileService.Communication;
using FileService.Contracts;
using FileService.Contracts.Options;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using SachkovTech.Core.Caching;

namespace FileService.IntegrationTests;

public class FileServiceCachingDecoratorTests : FileServiceTestsBase
{
    private readonly ICacheService _cacheService;

    public FileServiceCachingDecoratorTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _cacheService = factory.Services.GetRequiredService<ICacheService>();
    }

    [Fact]
    public async Task FileServiceCachingDecorator_GetDownloadUrl_CachesAndReturnsUrlCorrectly()
    {
        // Arrange
        var fileId = Guid.NewGuid().ToString();
        var bucketName = "test-bucket";
        var url = "https://example.com";
        var cacheKey = $"{fileId}";

        var fileServiceCachingDecorator = CreateDecoratorMoqDownloadUrl(
            new FileUrl(fileId, url), bucketName);

        var request = new GetDownloadUrlRequest(fileId, bucketName);

        // Act
        var result = await fileServiceCachingDecorator.GetDownloadUrl(request, CancellationToken.None);

        // Assert
        result.Value.DownloadUrl.Should().Be(url);

        var cachedUrl = await _cacheService.GetAsync<string>(cacheKey, CancellationToken.None);
        cachedUrl.Should().Be(url);
    }

    [Fact]
    public async Task FileServiceCachingDecorator_GetDownloadUrl_ReturnsCachedUrlCorrectly()
    {
        // Arrange
        var fileId = Guid.NewGuid().ToString();
        var bucketName = "test-bucket";
        var url = "https://example.com";
        var cacheKey = $"{fileId}";

        var fileServiceCachingDecorator = CreateDecoratorMoqDownloadUrl(
            new FileUrl(fileId, url), bucketName);

        var request = new GetDownloadUrlRequest(fileId, bucketName);

        await _cacheService.SetAsync(
            cacheKey,
            url,
            new DistributedCacheEntryOptions(),
            CancellationToken.None);

        // Act
        var result = await fileServiceCachingDecorator.GetDownloadUrl(request, CancellationToken.None);

        // Assert
        result.Value.DownloadUrl.Should().Be(url);
    }

    [Fact]
    public async Task FileServiceCachingDecorator_GetDownloadUrls_CachesAndReturnsUrlsCorrectly()
    {
        // Arrange
        var fileId1 = Guid.NewGuid().ToString();
        var fileId2 = Guid.NewGuid().ToString();
        var bucketName = "test-bucket";
        var url1 = "https://example.com/1";
        var url2 = "https://example.com/2";

        var cacheKey1 = $"{fileId1}";
        var cacheKey2 = $"{fileId2}";

        var fileServiceCachingDecorator = CreateDecoratorMoqDownloadUrls(
            new FileUrl[] { new FileUrl(fileId1, url1), new FileUrl(fileId2, url2) }, bucketName);

        var request = new GetDownloadUrlsRequest(new[]
        {
            new FileLocation(fileId1, bucketName), new FileLocation(fileId2, bucketName),
        });

        // Act
        var result = await fileServiceCachingDecorator.GetDownloadUrls(request, CancellationToken.None);

        // Assert
        result.Value.FileUrls.Should().HaveCount(2);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId1 && x.Url == url1);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId2 && x.Url == url2);

        var cachedUrl1 = await _cacheService.GetAsync<string>(cacheKey1, CancellationToken.None);
        cachedUrl1.Should().Be(url1);
        var cachedUrl2 = await _cacheService.GetAsync<string>(cacheKey2, CancellationToken.None);
        cachedUrl2.Should().Be(url2);
    }

    [Fact]
    public async Task FileServiceCachingDecorator_GetDownloadUrls_ReturnsCachedUrlsCorrectly()
    {
        // Arrange
        var fileId1 = Guid.NewGuid().ToString();
        var fileId2 = Guid.NewGuid().ToString();
        var bucketName = "test-bucket";
        var url1 = "https://example.com/1";
        var url2 = "https://example.com/2";

        var cacheKey1 = $"{fileId1}";
        var cacheKey2 = $"{fileId2}";

        var fileServiceCachingDecorator = CreateDecoratorMoqDownloadUrls(
            [], bucketName);

        var request = new GetDownloadUrlsRequest(new[]
        {
            new FileLocation(fileId1, bucketName), new FileLocation(fileId2, bucketName),
        });

        await _cacheService.SetAsync(
            cacheKey1,
            url1,
            new DistributedCacheEntryOptions(),
            CancellationToken.None);
        await _cacheService.SetAsync(
            cacheKey2,
            url2,
            new DistributedCacheEntryOptions(),
            CancellationToken.None);

        // Act
        var result = await fileServiceCachingDecorator.GetDownloadUrls(request, CancellationToken.None);

        // Assert
        result.Value.FileUrls.Should().HaveCount(2);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId1 && x.Url == url1);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId2 && x.Url == url2);
    }

    [Fact]
    public async Task FileServiceCachingDecorator_GetDownloadUrls_ReturnsCachedAndUncachedUrlsCorrectly()
    {
        // Arrange
        var fileId1 = Guid.NewGuid().ToString();
        var fileId2 = Guid.NewGuid().ToString();
        var fileId3 = Guid.NewGuid().ToString();
        var bucketName = "test-bucket";
        var url1 = "https://example.com/1";
        var url2 = "https://example.com/2";
        var url3 = "https://example.com/3";

        var cacheKey1 = $"{fileId1}";
        var cacheKey2 = $"{fileId2}";

        var fileServiceCachingDecorator = CreateDecoratorMoqDownloadUrls(
            new FileUrl[] { new FileUrl(fileId3, url3) },
            bucketName);

        var request = new GetDownloadUrlsRequest(new[]
        {
            new FileLocation(fileId1, bucketName),
            new FileLocation(fileId2, bucketName),
            new FileLocation(fileId3, bucketName),
        });

        await _cacheService.SetAsync(
            cacheKey1,
            url1,
            new DistributedCacheEntryOptions(),
            CancellationToken.None);

        await _cacheService.SetAsync(
            cacheKey2,
            url2,
            new DistributedCacheEntryOptions(),
            CancellationToken.None);

        // Act
        var result = await fileServiceCachingDecorator.GetDownloadUrls(request, CancellationToken.None);

        // Assert
        result.Value.FileUrls.Should().HaveCount(3);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId1 && x.Url == url1);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId2 && x.Url == url2);
        result.Value.FileUrls.Should().Contain(x => x.FileId == fileId3 && x.Url == url3);
    }

    private FileServiceCachingDecorator CreateFileServiceCachingDecorator(
        Mock<IFileService> mockFileService)
    {
        var minioOptions = new MinioOptions { UrlExpirationDays = 6 };

        var optionsWrapper = Options.Create(minioOptions);

        var fileServiceCachingDecorator = new FileServiceCachingDecorator(
            mockFileService.Object,
            _cacheService,
            optionsWrapper);

        return fileServiceCachingDecorator;
    }

    private FileServiceCachingDecorator CreateDecoratorMoqDownloadUrl(
        FileUrl fileUrl,
        string bucketName)
    {
        var mockFileService = new Mock<IFileService>();

        mockFileService
            .Setup(f => f.GetDownloadUrl(
                It.Is<GetDownloadUrlRequest>(r => r.FileId == fileUrl.FileId && r.BucketName == bucketName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDownloadUrlResponse(fileUrl.Url));

        return CreateFileServiceCachingDecorator(mockFileService);
    }

    private FileServiceCachingDecorator CreateDecoratorMoqDownloadUrls(
        FileUrl[] fileUrl,
        string bucketName)
    {
        var mockFileService = new Mock<IFileService>();

        mockFileService
            .Setup(f => f.GetDownloadUrls(
                It.Is<GetDownloadUrlsRequest>(r =>
                    r.Locations.Any(l => l.FileId == fileUrl[0].FileId && l.BucketName == bucketName)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDownloadUrlsResponse(fileUrl));

        return CreateFileServiceCachingDecorator(mockFileService);
    }
}