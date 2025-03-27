namespace FaqService.Constants;

public static class Constants
{
    public const int MAX_TEXT_LENGTH = 5000;
    public const int LOW_TEXT_LENGTH = 255;

    public const string LINK_PATTERN =
        @"^(https:\/\/|http:\/\/)?(www\.)?github\.com\/[a-zA-Z0-9_-]+\/[a-zA-Z0-9_.-]+\/?$";
}