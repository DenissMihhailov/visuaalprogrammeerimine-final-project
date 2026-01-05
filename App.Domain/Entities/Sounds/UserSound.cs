using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;

namespace App.Domain.Entities.Sounds;

public sealed class UserSound : SoundBase
{
    public string FilePath { get; protected set; } = "";

    private UserSound() { }

    public UserSound(Guid id, string name, TrackRole role, string filePath, DateTime createdAt)
        : base(id, name, role, createdAt)
    {
        FilePath = filePath;
    }
}

