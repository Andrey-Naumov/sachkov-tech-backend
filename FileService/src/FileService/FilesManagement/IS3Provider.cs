using Amazon.S3.Model;
using FileService.Contracts;

namespace FileService.FilesManagement;

public interface IS3Provider
{
    /// <summary>
    /// Получение списка всех multipart-загрузок в указанном бакете.
    /// </summary>
    /// <param name="bucketName">Имя бакета.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ со списком multipart-загрузок.</returns>
    Task<ListMultipartUploadsResponse> ListMultipartUploadAsync(string bucketName, CancellationToken cancellationToken);

    /// <summary>
    /// Получение списка всех бакетов в аккаунте.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список имён бакетов.</returns>
    Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Инициализация multipart-загрузки
    /// </summary>
    /// <param name="fileName">Название файла.</param>
    /// <param name="contentType">Тип файла.</param>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ multipart-загрузки из S3 клиента.</returns>
    Task<string> StartMultipartUpload(
        string fileName,
        string contentType,
        FileLocation fileLocation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Генерация предподписанной ссылки для загрузки чанка
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="uploadId">Идентификатор multipart-загрузки.</param>
    /// <param name="partNumber">Номер части.</param>
    /// <returns>Предподписанная ссылка для загрузки чанка.</returns>
    Task<string> GenerateChunkUploadUrl(
        FileLocation fileLocation,
        string uploadId,
        int partNumber);

    /// <summary>
    /// Генерация предподписанной ссылки для файла.
    /// </summary>
    /// <param name="fileName">Название файла.</param>
    /// <param name="location">Локация файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Предподписанная ссылка для загрузки чанка.</returns>
    Task<string> GenerateUploadUrl(
        string fileName,
        FileLocation location,
        CancellationToken cancellationToken);

    /// <summary>
    /// Завершение multipart-загрузки
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="uploadId">Идентификатор multipart-загрузки.</param>
    /// <param name="partETags">Список тегов чанков.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>S3 ключ.</returns>
    Task<string> CompleteMultipartUploadAsync(
        FileLocation fileLocation,
        string uploadId,
        List<(int PartNumber, string ETag)> partETags,
        CancellationToken cancellationToken);

    /// <summary>
    /// Прерывание multipart-загрузки.
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="uploadId">Идентификатор multipart-загрузки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Task.</returns>
    Task AbortMultipartUploadAsync(
        FileLocation fileLocation,
        string uploadId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Генерация ссылки на скачивание файла.
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="expirationHours">Время жизни ссылки.</param>
    /// <returns>Ссылка на скачивание файла.</returns>
    Task<string> GenerateDownloadUrlAsync(FileLocation fileLocation, int expirationHours);

    /// <summary>
    /// Генерация ссылок на скачивание файлов.
    /// </summary>
    /// <param name="fileIds">Идентификаторы файлов.</param>
    /// <param name="expirationHours">Время жизни ссылки.</param>
    /// <returns>Ссылки и id для скачивания файлов.</returns>
    Task<IReadOnlyList<FileUrl?>> GenerateDownloadUrlsAsync(IEnumerable<FileLocation> fileIds, int expirationHours);

    /// <summary>
    /// Загрузка файла из S3 в указанную дерикторию.
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="tempInputPath">Временная директория для хранения файла.</param>
    /// <param name="cancellationToken">Токен отметы.</param>
    /// <returns>Путь загруженного файла.</returns>
    Task<string> DownloadFileAsync(FileLocation fileLocation, string tempInputPath, CancellationToken cancellationToken);

    /// <summary>
    /// Загрузка файла в бакет с помощью Stream.
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="contentType">Контент-тип.</param>
    /// <param name="file">Поток файла.</param>
    /// <param name="cancellationToken">Токен отметы.</param>
    /// <returns>Task.</returns>
    Task UploadFileAsync(FileLocation fileLocation, string? contentType, Stream file, CancellationToken cancellationToken);

    /// <summary>
    /// Удаление файла из S3.
    /// </summary>
    /// <param name="fileLocation">Локация файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Id удалённого файла.</returns>
    Task<string> DeleteFileAsync(FileLocation fileLocation, CancellationToken cancellationToken);
}