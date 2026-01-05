using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;

namespace App.Domain.Entities.Kits;

public sealed class KitSlot
{
    public Guid Id { get; private set; }
    public Guid KitId { get; private set; }
    public TrackRole Role { get; private set; }
    public Guid? SoundId { get; private set; }

    private KitSlot() { }

    public KitSlot(Guid id, Guid kitId, TrackRole role, Guid? soundId)
    {
        Id = id;
        KitId = kitId;
        Role = role;
        SoundId = soundId;
    }

    public void SetSound(Guid soundId) => SoundId = soundId;
}

