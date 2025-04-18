using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace SachkovTech.Core.Database;

public sealed record Cursor<TFilter, TId>(TFilter Filter, TId LastId)
    where TFilter : IComparable
    where TId : IComparable<TId>
{
    public static string Encode(TFilter filter, TId lastId)
    {
        var cursor = new Cursor<TFilter, TId>(filter, lastId);
        string json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static Cursor<TFilter, TId>? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return JsonSerializer.Deserialize<Cursor<TFilter, TId>>(json);
        }
        catch
        {
            return null;
        }
    }
}