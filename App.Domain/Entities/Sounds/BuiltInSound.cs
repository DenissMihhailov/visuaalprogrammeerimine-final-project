using App.Domain.Enums;

namespace App.Domain.Entities.Sounds;

public sealed class BuiltInSound : SoundBase
{
    public string ResourcePath { get; protected set; } = "";

    private BuiltInSound() { }

    public BuiltInSound(Guid id, string name, TrackRole role, string resourcePath, DateTime createdAt)
        : base(id, name, role, createdAt)
    {
        ResourcePath = resourcePath;
    }
}
