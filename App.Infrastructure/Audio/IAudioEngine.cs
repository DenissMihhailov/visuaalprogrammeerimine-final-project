using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Entities.Sounds;

namespace App.Infrastructure.Audio;

public interface IAudioEngine
{
    void Play(SoundBase sound);
    void SetMasterVolume(float volume01); 
}
