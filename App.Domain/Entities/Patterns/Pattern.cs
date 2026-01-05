using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;

namespace App.Domain.Entities.Patterns;

public sealed class Pattern
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public int StepsCount { get; private set; }
    public int Bpm { get; private set; }
    public PatternStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PatternStep> _steps = new();
    public IReadOnlyList<PatternStep> Steps => _steps;

    private Pattern() { }

    public Pattern(Guid id, string code, string name, int stepsCount, int bpm, DateTime createdAt)
    {
        Id = id;
        Code = code;
        Name = name;
        StepsCount = stepsCount;
        SetBpm(bpm);
        Status = PatternStatus.Draft;
        CreatedAt = createdAt;

        foreach (var role in Enum.GetValues<TrackRole>())
            for (var i = 0; i < stepsCount; i++)
                _steps.Add(new PatternStep(Guid.NewGuid(), id, role, i, false));
    }

    public void Rename(string name) => Name = name;

    public void SetBpm(int bpm)
    {
        if (bpm < 40 || bpm > 240) throw new InvalidOperationException("BPM out of range");
        Bpm = bpm;
    }

    public void Toggle(TrackRole role, int stepIndex)
    {
        if (Status == PatternStatus.Locked) throw new InvalidOperationException("Pattern is locked");
        var s = _steps.First(x => x.Role == role && x.StepIndex == stepIndex);
        s.SetOn(!s.IsOn);
    }

    public void SetStatus(PatternStatus status)
    {
        if (status == PatternStatus.Ready && !_steps.Any(x => x.IsOn))
            throw new InvalidOperationException("Cannot set Ready: empty pattern");

        if (Status == PatternStatus.Locked && status != PatternStatus.Locked)
            throw new InvalidOperationException("Locked pattern cannot be changed");

        Status = status;
    }
}

