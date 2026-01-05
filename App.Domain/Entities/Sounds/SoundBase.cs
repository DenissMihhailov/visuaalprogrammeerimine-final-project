using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;

namespace App.Domain.Entities.Sounds;

public abstract class SoundBase
{
    public Guid Id { get; protected set; }
    public string Name { get; protected set; } = "";
    public TrackRole Role { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    protected SoundBase() { }

    protected SoundBase(Guid id, string name, TrackRole role, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Role = role;
        CreatedAt = createdAt;
    }
}
