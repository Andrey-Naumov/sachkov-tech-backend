using FileService.Contracts;

namespace FileService.VideoProcessing;

public class VideoProcessingContext
{
    public required FileLocation VideoLocation { get; init; }

    public string TempDirectory { get; set; } = string.Empty;

    public string InputPath { get; set; } = string.Empty;

    public string HlsOutputPath { get; set; } = string.Empty;

    public Guid? HlsGuid { get; set; }

    public Guid? PreviewGuid { get; set; }

    public required AsyncProgress<double> Progress { get; init; }

    public double CurrentProgress { get; set; } = 0;
}