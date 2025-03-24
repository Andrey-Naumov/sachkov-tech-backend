namespace FileService.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Нормализует аргументы команды для удобства чтения.
    /// </summary>
    /// <param name="input">Входная строка с аргументами.</param>
    /// <param name="separator">Разделитель между аргументами (по умолчанию — пробел).</param>
    /// <returns>Нормализованная строка.</returns>
    public static string NormalizeView(this string input, string separator = " ")
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Удаляем лишние пробелы и разделяем аргументы
        string[] args = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        // Собираем аргументы обратно в строку с указанным разделителем
        return string.Join(separator, args);
    }
}