using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;
using App.Domain.Entities.Sounds;

namespace App.Domain.Entities.Kits;

public sealed class Kit
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }

    public List<KitSlot> Slots { get; private set; } = new();


    private Kit() { }

    public Kit(Guid id, string name, DateTime createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;

        foreach (var role in Enum.GetValues<TrackRole>())
            Slots.Add(new KitSlot(Guid.NewGuid(), id, role, null));
    }

    public void Rename(string name) => Name = name;

    public void Assign(TrackRole role, SoundBase sound)
    {
        var slot = Slots.First(x => x.Role == role);
        slot.SetSound(sound.Id);
    }
}

