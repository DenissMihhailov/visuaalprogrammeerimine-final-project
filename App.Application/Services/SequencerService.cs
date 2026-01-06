using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Entities.Kits;
using App.Domain.Entities.Patterns;
using App.Domain.Entities.Sounds;
using App.Domain.Enums;
using App.Infrastructure.Audio;
using App.Infrastructure.Data;
using App.Infrastructure.Export;
using Microsoft.EntityFrameworkCore;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace App.Application.Services;

public sealed class SequencerService
{
    private readonly AppDbContext _db;
    private readonly IAudioEngine _audio;
    private readonly CsvExporter _csv;
    private CancellationTokenSource? _cts;
    private Task? _playTask;

    public SequencerService(AppDbContext db, IAudioEngine audio, CsvExporter csv)
    {
        _db = db;
        _audio = audio;
        _csv = csv;
    }

    public async Task<List<Pattern>> GetPatternsAsync() =>
        await _db.Patterns.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<Pattern> GetPatternWithStepsAsync(Guid id) =>
        await _db.Patterns.Include(x => x.Steps).AsNoTracking().FirstAsync(x => x.Id == id);

    public async Task<List<Kit>> GetKitsAsync() =>
        await _db.Kits.Include(x => x.Slots).AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<List<SoundBase>> GetSoundsAsync(TrackRole? role = null)
    {
        var q = _db.Sounds.AsNoTracking().AsQueryable();
        if (role is not null) q = q.Where(x => x.Role == role);
        return await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task ToggleStepAsync(Guid patternId, TrackRole role, int stepIndex)
    {
        var p = await _db.Patterns.Include(x => x.Steps).FirstAsync(x => x.Id == patternId);
        p.Toggle(role, stepIndex);
        await _db.SaveChangesAsync();
    }

    public async Task SetBpmAsync(Guid patternId, int bpm)
    {
        var p = await _db.Patterns.FirstAsync(x => x.Id == patternId);
        p.SetBpm(bpm);
        await _db.SaveChangesAsync();
    }

    public async Task ChangePatternStatusAsync(Guid patternId, PatternStatus status)
    {
        var p = await _db.Patterns.Include(x => x.Steps).FirstAsync(x => x.Id == patternId);
        p.SetStatus(status);
        await _db.SaveChangesAsync();
    }

    public async Task<Guid> CreatePatternAsync(string code, string name)
    {
        var p = new Pattern(Guid.NewGuid(), code, name, 16, 128, DateTime.UtcNow);
        _db.Patterns.Add(p);
        await _db.SaveChangesAsync();
        return p.Id;
    }

    public async Task DeletePatternAsync(Guid id)
    {
        var p = await _db.Patterns.FirstAsync(x => x.Id == id);
        _db.Patterns.Remove(p);
        await _db.SaveChangesAsync();
    }

    public async Task<Guid> CreateKitAsync(string name)
    {
        var k = new Kit(Guid.NewGuid(), name, DateTime.UtcNow);
        _db.Kits.Add(k);
        await _db.SaveChangesAsync();
        return k.Id;
    }

    public async Task SetKitSoundAsync(Guid kitId, TrackRole role, Guid soundId)
    {
        var kit = await _db.Kits.Include(x => x.Slots).FirstAsync(x => x.Id == kitId);
        var slot = kit.Slots.First(x => x.Role == role);
        slot.SetSound(soundId);
        await _db.SaveChangesAsync();
    }

    public async Task<Kit> GetKitWithSlotsAsync(Guid kitId) =>
        await _db.Kits.Include(x => x.Slots).AsNoTracking().FirstAsync(x => x.Id == kitId);




    public async Task DeleteKitAsync(Guid id)
    {
        var k = await _db.Kits.FirstAsync(x => x.Id == id);
        _db.Kits.Remove(k);
        await _db.SaveChangesAsync();
    }

    public async Task AssignKitSlotAsync(Guid kitId, TrackRole role, Guid soundId)
    {
        var kit = await _db.Kits.Include(x => x.Slots).FirstAsync(x => x.Id == kitId);
        var slot = kit.Slots.First(x => x.Role == role);
        slot.SetSound(soundId);
        await _db.SaveChangesAsync();
    }

    public async Task PlayRoleFromKitAsync(Guid kitId, TrackRole role)
    {
        var kit = await _db.Kits.Include(x => x.Slots).AsNoTracking().FirstAsync(x => x.Id == kitId);
        var sid = kit.Slots.First(x => x.Role == role).SoundId;
        if (sid is null) return;

        var s = await _db.Sounds.AsNoTracking().FirstAsync(x => x.Id == sid.Value);
        _audio.Play(s);
    }

    public async Task StartPatternAsync(Guid patternId, Guid kitId)
    {
        if (_playTask is not null && !_playTask.IsCompleted) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        var pattern = await _db.Patterns.Include(x => x.Steps).AsNoTracking()
            .FirstAsync(x => x.Id == patternId);

        var kit = await _db.Kits.Include(x => x.Slots).AsNoTracking()
            .FirstAsync(x => x.Id == kitId);

        var ids = kit.Slots.Where(s => s.SoundId != null).Select(s => s.SoundId!.Value).Distinct().ToArray();

        var sounds = await _db.Sounds.AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();

        var roleToSound = kit.Slots
            .Where(s => s.SoundId != null)
            .Join(sounds, slot => slot.SoundId!.Value, snd => snd.Id, (slot, snd) => new { slot.Role, snd })
            .ToDictionary(x => x.Role, x => x.snd);

        var stepMs = TimeSpan.FromMilliseconds(60000.0 / pattern.Bpm / 4.0);

        _playTask = Task.Run(async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long n = 0;

            while (!token.IsCancellationRequested)
            {
                var target = TimeSpan.FromTicks(stepMs.Ticks * n);
                var wait = target - sw.Elapsed;
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait, token);

                var stepIndex = (int)(n % pattern.StepsCount);

                foreach (var h in pattern.Steps)
                    if (h.StepIndex == stepIndex && h.IsOn && roleToSound.TryGetValue(h.Role, out var snd))
                        _audio.Play(snd);

                n++;
            }
        }, token);
    }


    public async Task<Guid> CreatePatternWithEmptyStepsAsync(string code, string name, int stepsCount = 16, int bpm = 128)
    {
        var p = new Pattern(Guid.NewGuid(), code, name, stepsCount, bpm, DateTime.UtcNow);
        _db.Patterns.Add(p);
        await _db.SaveChangesAsync();
        return p.Id;
    }


    public async Task SavePatternStateAsync(Guid patternId, int bpm, IReadOnlyDictionary<TrackRole, bool[]> roleSteps)
    {
        var p = await _db.Patterns.Include(x => x.Steps).FirstAsync(x => x.Id == patternId);

        p.SetBpm(bpm);

        foreach (var st in p.Steps)
            st.SetOn(roleSteps[st.Role][st.StepIndex]);

        await _db.SaveChangesAsync();
    }

    public async Task SaveKitStateAsync(Guid kitId, IReadOnlyDictionary<TrackRole, Guid> roleToSoundId)
    {
        var kit = await _db.Kits.Include(x => x.Slots).FirstAsync(x => x.Id == kitId);

        foreach (var slot in kit.Slots)
            if (roleToSoundId.TryGetValue(slot.Role, out var sid))
                slot.SetSound(sid);

        await _db.SaveChangesAsync();
    }


    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

}
