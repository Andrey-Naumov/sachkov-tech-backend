namespace FileService.FilesManagement;

public static class ChunkSizeCalculator
{
    private const long TARGET_CHUNK_SIZE = 100 * 1024 * 1024; // Целевой размер чанка — 100 MB
    private const int MAX_CHUNKS = 10_000; // Максимальное количество чанков

    /// <summary>
    /// Рассчитывает параметры чанков: размер одного чанка и их количество
    /// </summary>
    /// <param name="fileSize">Размер файла в байтах.</param>
    /// <returns>Размер чанка и общее количество чанков.</returns>
    public static (long ChunkSize, int TotalChunks) Calculate(long fileSize)
    {
        // Если файл меньше или равен 100 МБ, возвращаем один чанк
        if (fileSize <= TARGET_CHUNK_SIZE)
        {
            return (fileSize, 1);
        }

        // Рассчитываем размер чанка
        long chunkSize = CalculateChunkSize(fileSize);

        // Рассчитываем количество чанков
        int totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

        return (chunkSize, totalChunks);
    }

    /// <summary>
    /// Рассчитывает размер чанка для файлов больше 100 МБ
    /// </summary>
    private static long CalculateChunkSize(long fileSize)
    {
        // Рассчитываем размер чанка так, чтобы он был близок к 100 МБ, но не превышал MaxChunks
        long chunkSize = (fileSize + MAX_CHUNKS - 1) / MAX_CHUNKS;

        // Округляем размер чанка до ближайшего кратного TargetChunkSize
        chunkSize = Math.Max(TARGET_CHUNK_SIZE, chunkSize);

        return chunkSize;
    }
}