using CSharpFunctionalExtensions;
using SharedKernel;

namespace TagService.Entities;

public class Tag : Entity<Guid>
{
    public const int TEXT_MAX_LENGTH = 1000;
    
    public string Name { get; private set; }
    
    public string Description { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public int UsagesCount { get; private set; }
    
    public Tag(Guid id, string name, string description, DateTime createdAt) : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    public static Result<Tag, Error> Create(string name, string description, DateTime createdAt)
    {
        if(string.IsNullOrWhiteSpace(name) || name.Length > TEXT_MAX_LENGTH)
            return Errors.General.ValueIsRequired("Name");
        
        if(string.IsNullOrWhiteSpace(description) || description.Length > TEXT_MAX_LENGTH)
            return Errors.General.ValueIsRequired("Description");
        
        return new Tag(Guid.NewGuid(), name, description, createdAt);
    }

    public UnitResult<Error> Edit(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > TEXT_MAX_LENGTH)
            return Errors.General.ValueIsInvalid("Text");
        
        if(string.IsNullOrWhiteSpace(description) || description.Length > TEXT_MAX_LENGTH)
            return Errors.General.ValueIsInvalid("Text");
        
        Name = name;
        Description = description;

        return Result.Success<Error>();
    }

    public void UsagesIncrease() => UsagesCount++;
    
    public UnitResult<Error> UsagesDecrease()
    {
        if (UsagesCount <= 0)
            return Errors.General.ValueIsInvalid("UsagesCount");
    
        UsagesCount--;
        return Result.Success<Error>();
    }
}