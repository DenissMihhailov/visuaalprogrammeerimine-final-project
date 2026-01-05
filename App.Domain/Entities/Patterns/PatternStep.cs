using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Domain.Enums;

namespace App.Domain.Entities.Patterns;

public sealed class PatternStep
{
    public Guid Id { get; private set; }
    public Guid PatternId { get; private set; }
    public TrackRole Role { get; private set; }
    public int StepIndex { get; private set; }
    public bool IsOn { get; private set; }

    private PatternStep() { }

    public PatternStep(Guid id, Guid patternId, TrackRole role, int stepIndex, bool isOn)
    {
        Id = id;
        PatternId = patternId;
        Role = role;
        StepIndex = stepIndex;
        IsOn = isOn;
    }

    public void SetOn(bool on) => IsOn = on;
}
