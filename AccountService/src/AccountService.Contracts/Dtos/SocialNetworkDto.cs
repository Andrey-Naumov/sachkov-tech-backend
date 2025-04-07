using System.Diagnostics.CodeAnalysis;

namespace AccountService.Contracts.Dtos;

public class SocialNetworkDto
{
    public SocialNetworkDto()
    {
    }

    [SetsRequiredMembers]
    public SocialNetworkDto(string link, string name)
    {
        Link = link;
        Name = name;
    }

    public required string Name { get; init; }

    public required string Link { get; init; }
}