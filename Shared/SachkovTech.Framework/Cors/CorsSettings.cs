namespace SachkovTech.Framework.Cors;

public class CorsSettings
{
    public const string CORS = "Cors";

    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowCredentials { get; set; } = true;

    public string[] AllowedHeaders { get; set; } = [];

    public string[] AllowedMethods { get; set; } = [];
}