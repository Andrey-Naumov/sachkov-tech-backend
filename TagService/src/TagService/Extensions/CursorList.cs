namespace TagService.Extensions;

public class CursorList<T>
{
    public IReadOnlyList<T> Items { get; init; }
    
    public string? Cursor { get; init; }
    
    public bool HasMore { get; init; }

    public CursorList(
        IEnumerable<T> items, 
        string? cursor, 
        bool hasMore)
    {
        Items = items.ToList();
        Cursor = cursor;
        HasMore = hasMore;
    }
}