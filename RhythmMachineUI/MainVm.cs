using App.Application.Services;
using App.Domain.Entities.Kits;
using App.Domain.Entities.Patterns;
using App.Domain.Entities.Sounds;
using App.Domain.Enums;
using App.Infrastructure.Audio;
using App.Infrastructure.Export;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RhythmMachineUI;

public sealed class MainVm : INotifyPropertyChanged
{
    private readonly SequencerService _svc;
    private readonly IAudioEngine _audio;
    private readonly CsvExporter _csv;


    private const int MinBpm = 40;
    private const int MaxBpm = 240;
    private int _lastGoodBpm = 128;
    private bool _initing;

    private int _currentStep;
    public int CurrentStep
    {
        get => _currentStep;
        set => Set(ref _currentStep, value);
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => Set(ref _isPlaying, value);
    }




    public ObservableCollection<ComboItemVm<Pattern>> Patterns { get; } = new();
    public ObservableCollection<ComboItemVm<Kit>> Kits { get; } = new();
    public ObservableCollection<RowVm> Rows { get; } = new();

    public ICommand ToggleCommand { get; }
    public ICommand SavePatternAsCommand { get; }
    public ICommand SaveKitAsCommand { get; }
    public ICommand DeletePatternCommand { get; }
    public ICommand DeleteKitCommand { get; }
    public ICommand SetBpmCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ExportPatternCsvCommand { get; }


    private ComboItemVm<Pattern>? _selectedPattern;
    public ComboItemVm<Pattern>? SelectedPattern
    {
        get => _selectedPattern;
        set
        {
            if (value is null) return;

           
            if (_selectedPattern?.IsDraft == true && !value.IsDraft)
                RemovePatternDraftIfAny();

            if (!Set(ref _selectedPattern, value)) return;
            if (_initing) return;
            _ = LoadPatternAsync();
        }
    }

    private ComboItemVm<Kit>? _selectedKit;
    public ComboItemVm<Kit>? SelectedKit
    {
        get => _selectedKit;
        set
        {
            if (value is null) return;

            if (_selectedKit?.IsDraft == true && !value.IsDraft)
                RemoveKitDraftIfAny();

            if (!Set(ref _selectedKit, value)) return;
            if (_initing) return;
            _ = LoadKitAsync();
        }
    }

    private ComboItemVm<Pattern>? _basePattern;
    private ComboItemVm<Kit>? _baseKit;

    private bool IsPatternDraft => SelectedPattern?.IsDraft == true;
    private bool IsKitDraft => SelectedKit?.IsDraft == true;

    private string _bpmText = "128";
    public string BpmText { get => _bpmText; set => Set(ref _bpmText, value); }

    private double _volume = 1.0;
    public double Volume
    {
        get => _volume;
        set
        {
            if (!Set(ref _volume, value)) return;
            _audio.SetMasterVolume((float)_volume);
        }
    }
    
    private bool _dbPlaying;
    private CancellationTokenSource? _previewCts;
    private CancellationTokenSource? _playheadCts;


    public MainVm(SequencerService svc, IAudioEngine audio, CsvExporter csv)
    {
        _svc = svc;
        _audio = audio;
        _csv = csv;

        ToggleCommand = new RelayCommand<StepCellVm>(async cell => await ToggleAsync(cell));
        SavePatternAsCommand = new RelayCommand(async () => await SavePatternAsAsync());
        SaveKitAsCommand = new RelayCommand(async () => await SaveKitAsAsync());
        DeletePatternCommand = new RelayCommand(async () => await DeletePatternAsync());
        DeleteKitCommand = new RelayCommand(async () => await DeleteKitAsync());
        SetBpmCommand = new RelayCommand(async () => await SetBpmAsync());
        PlayCommand = new RelayCommand(async () => await PlayAsync());
        ExportPatternCsvCommand = new RelayCommand(async () => await ExportPatternCsvAsync());


        StopCommand = new RelayCommand(() =>
        {
            _svc.Stop();
            _dbPlaying = false;

            _previewCts?.Cancel();
            _previewCts = null;

            _playheadCts?.Cancel();
            _playheadCts = null;

            IsPlaying = false;
            CurrentStep = 0;
        });



        _audio.SetMasterVolume(1.0f);

        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        _initing = true;

        await ReloadPatternsAsync();
        await ReloadKitsAsync();


        _selectedPattern = Patterns.LastOrDefault(p => !p.IsDraft);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPattern)));

        _selectedKit = Kits.LastOrDefault(k => !k.IsDraft);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKit)));


        _initing = false;

        await LoadPatternAsync();
        await LoadKitAsync();
    }


    private async Task ReloadPatternsAsync()
    {
        var patterns = await _svc.GetPatternsAsync();
        Patterns.Clear();
        foreach (var p in patterns)
            Patterns.Add(new ComboItemVm<Pattern>(p, p.Name));
    }

    private async Task ReloadKitsAsync()
    {
        var kits = await _svc.GetKitsAsync();
        Kits.Clear();
        foreach (var k in kits)
            Kits.Add(new ComboItemVm<Kit>(k, k.Name));
    }

    private void EnsurePatternDraft()
    {
        if (SelectedPattern is null) return;
        if (SelectedPattern.IsDraft) return;

        _basePattern = SelectedPattern;

        Patterns.Insert(0, ComboItemVm<Pattern>.Draft());
        SelectedPattern = Patterns[0];

        if (_dbPlaying)
        {
            _svc.Stop();
            _dbPlaying = false;
            _ = StartPreviewFromUiAsync();
        }
    }

    private void EnsureKitDraft()
    {
        if (SelectedKit is null) return;
        if (SelectedKit.IsDraft) return;

        _baseKit = SelectedKit;

        Kits.Insert(0, ComboItemVm<Kit>.Draft());
        SelectedKit = Kits[0];

        if (_dbPlaying)
        {
            _svc.Stop();
            _dbPlaying = false;
            _ = StartPreviewFromUiAsync();
        }
    }

    private async Task LoadPatternAsync()
    {

        var prevSelected = Rows
        .Where(r => r.SelectedSound is not null)
        .ToDictionary(r => r.Role, r => r.SelectedSound!.Id);

        if (SelectedPattern is null) return;

        if (SelectedPattern.IsDraft) return;

        var p = await _svc.GetPatternWithStepsAsync(SelectedPattern.Model!.Id);

        _lastGoodBpm = p.Bpm;
        BpmText = p.Bpm.ToString();



        var sounds = await _svc.GetSoundsAsync();

        Rows.Clear();
        foreach (var role in Enum.GetValues<TrackRole>())
        {
            var rowSounds = sounds.Where(s => s.Role == role).ToList();
            var row = new RowVm(role, rowSounds);

            row.Changed += () => EnsureKitDraft();

            for (int i = 0; i < p.StepsCount; i++)
            {
                var isOn = p.Steps.Any(s => s.Role == role && s.StepIndex == i && s.IsOn);
                row.Steps.Add(new StepCellVm(role, i, isOn));
            }

            Rows.Add(row);
        }

        foreach (var row in Rows)
        {
            if (prevSelected.TryGetValue(row.Role, out var sid))
            {
                var found = row.Sounds.FirstOrDefault(s => s.Id == sid);
                if (found is not null)
                    row.SetSelectedSoundFromDb(found);
            }
        }

        await LoadKitAsync();

        if (IsPlaying)
        {
            _svc.Stop();
            _dbPlaying = false;
            await StartPreviewFromUiAsync();
        }

    }

    private async Task LoadKitAsync()
    {
        if (SelectedKit is null) return;
        if (SelectedKit.IsDraft) return;

        var kit = await _svc.GetKitWithSlotsAsync(SelectedKit.Model!.Id);

        foreach (var row in Rows)
        {
            var sid = kit.Slots.First(x => x.Role == row.Role).SoundId;
            var s = sid is null
                ? row.Sounds.FirstOrDefault()
                : row.Sounds.FirstOrDefault(x => x.Id == sid.Value) ?? row.Sounds.FirstOrDefault();

            row.SetSelectedSoundFromDb(s);
        }
    }

    private async Task ToggleAsync(StepCellVm? cell)
    {
        if (cell is null) return;

        EnsurePatternDraft();

        if (!IsPlaying)
        {
            var row = Rows.First(r => r.Role == cell.Role);
            var snd = row.SelectedSound;
            if (snd is not null)
                _audio.Play(snd);
        }

        await Task.CompletedTask;
    }


    private void StartPlayhead()
    {
        var bpm = GetBpmOrRevert(showDialog: false);


        _playheadCts?.Cancel();
        _playheadCts = new CancellationTokenSource();
        var token = _playheadCts.Token;

        var stepsCount = Rows.FirstOrDefault()?.Steps.Count ?? 16;
        var stepMs = TimeSpan.FromMilliseconds(60000.0 / bpm / 4.0);

        _ = Task.Run(async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long n = 0;

            while (!token.IsCancellationRequested && IsPlaying)
            {
                var target = TimeSpan.FromTicks(stepMs.Ticks * n);
                var wait = target - sw.Elapsed;
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait, token);

                var idx = (int)(n % stepsCount);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentStep = idx;
                });

                n++;
            }
        }, token);
    }



    private async Task SetBpmAsync()
    {
        if (!TryGetValidBpm(out var bpm, showDialog: true))
        {
            BpmText = _lastGoodBpm.ToString();
            return;
        }

        EnsurePatternDraft();

        _lastGoodBpm = bpm;

        await Task.CompletedTask;
    }

    private bool ValidateName(string name, IEnumerable<string> existingNames, string entityTitle)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            System.Windows.MessageBox.Show(
                $"{entityTitle} name cannot be empty.",
                "Invalid name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }

        if (name.Length < 2)
        {
            System.Windows.MessageBox.Show(
                $"{entityTitle} name is too short.",
                "Invalid name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }

        if (!name.Any(char.IsLetterOrDigit))
        {
            System.Windows.MessageBox.Show(
                $"{entityTitle} name must contain letters or digits.",
                "Invalid name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }

        if (existingNames.Any(x => string.Equals(x?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
        {
            System.Windows.MessageBox.Show(
                $"{entityTitle} with this name already exists.",
                "Duplicate name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }

        return true;
    }


    private bool TryGetValidBpm(out int bpm, bool showDialog)
    {
        if (!int.TryParse(BpmText, out bpm))
        {
            if (showDialog)
                System.Windows.MessageBox.Show(
                    "BPM must be a number.",
                    "Invalid BPM",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

            bpm = _lastGoodBpm;
            return false;
        }

        if (bpm < MinBpm || bpm > MaxBpm)
        {
            if (showDialog)
                System.Windows.MessageBox.Show(
                    $"BPM must be between {MinBpm} and {MaxBpm}.",
                    "BPM out of range",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

            bpm = _lastGoodBpm;
            return false;
        }

        return true;
    }


    private async Task ExportPatternCsvAsync()
    {
        if (SelectedPattern is null) return;

        if (SelectedPattern.IsDraft)
        {
            var res = System.Windows.MessageBox.Show(
                "Pattern is not saved. Export current draft?",
                "Export CSV",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (res != System.Windows.MessageBoxResult.Yes)
                return;
        }

        var bpm = int.TryParse(BpmText, out var b) ? b : _lastGoodBpm;

        var map = new Dictionary<TrackRole, bool[]>();
        foreach (var row in Rows)
        {
            var arr = new bool[row.Steps.Count];
            for (int i = 0; i < row.Steps.Count; i++)
                arr[i] = row.Steps[i].IsOn;

            map[row.Role] = arr;
        }

        var stepsCount = Rows.FirstOrDefault()?.Steps.Count ?? 16;

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var safeName = MakeSafeFileName(SelectedPattern.DisplayName);
        var filePath = System.IO.Path.Combine(desktop, $"{safeName}.csv");

        _csv.ExportPattern(filePath, bpm, stepsCount, map);

        System.Windows.MessageBox.Show(
            $"Saved to Desktop:\n{filePath}",
            "Export CSV",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);

        await Task.CompletedTask;
    }

    private static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "pattern";
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }


    private async Task PlayAsync()
    {
        System.Windows.Input.Keyboard.ClearFocus();

        if (SelectedPattern is null || SelectedKit is null) return;

        IsPlaying = true;

        _svc.Stop();
        _dbPlaying = false;

        await StartPreviewFromUiAsync();
    }

    private int GetBpmOrRevert(bool showDialog)
    {
        if (TryGetValidBpm(out var bpm, showDialog))
        {
            _lastGoodBpm = bpm;
            return bpm;
        }

        BpmText = _lastGoodBpm.ToString();
        return _lastGoodBpm;
    }


    private async Task StartPreviewFromUiAsync()
    {
        var bpm = GetBpmOrRevert(showDialog: true);

        _previewCts?.Cancel();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;


        var stepsCount = Rows.FirstOrDefault()?.Steps.Count ?? 16;

        var stepMs = TimeSpan.FromMilliseconds(60000.0 / bpm / 4.0);

        _ = Task.Run(async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long n = 0;

            while (!token.IsCancellationRequested)
            {
                var target = TimeSpan.FromTicks(stepMs.Ticks * n);
                var wait = target - sw.Elapsed;
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait, token);

                var stepIndex = (int)(n % stepsCount);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentStep = stepIndex;
                });

                foreach (var row in Rows)
                {
                    if (stepIndex >= row.Steps.Count) continue;

                    var snd = row.SelectedSound;
                    if (row.Steps[stepIndex].IsOn && snd is not null)
                        _audio.Play(snd);
                }


                n++;
            }
        }, token);

        await Task.CompletedTask;
    }

    private async Task SavePatternAsAsync()
    {
        var dlg = new NameDialog("Pattern name:");
        dlg.Owner = System.Windows.Application.Current.MainWindow;
        if (dlg.ShowDialog() != true) return;

        var name = dlg.Name.Trim();

        if (!ValidateName(
                name,
                Patterns.Where(x => !x.IsDraft).Select(x => x.DisplayName),
                "Pattern"))
            return;


        var bpm = int.TryParse(BpmText, out var b) ? b : 128;
        var code = $"PAT-{DateTime.UtcNow:MMdd-HHmm}";

        var newId = await _svc.CreatePatternWithEmptyStepsAsync(code, name, 16, bpm);

        var map = new Dictionary<TrackRole, bool[]>();
        foreach (var row in Rows)
        {
            var arr = new bool[row.Steps.Count];
            for (int i = 0; i < row.Steps.Count; i++)
                arr[i] = row.Steps[i].IsOn;

            map[row.Role] = arr;
        }

        await _svc.SavePatternStateAsync(newId, bpm, map);

        await ReloadPatternsAsync();
        SelectedPattern = Patterns.First(x => x.Model!.Id == newId);

        _basePattern = null;
    }

    private async Task SaveKitAsAsync()
    {
        var dlg = new NameDialog("Kit name:");
        dlg.Owner = System.Windows.Application.Current.MainWindow;
        if (dlg.ShowDialog() != true) return;

        var name = dlg.Name.Trim();

        if (!ValidateName(
                name,
                Kits.Where(x => !x.IsDraft).Select(x => x.DisplayName),
                "Kit"))
            return;

        var newId = await _svc.CreateKitAsync(name);

        var map = new Dictionary<TrackRole, Guid>();
        foreach (var row in Rows)
        {
            var snd = row.SelectedSound ?? row.Sounds.FirstOrDefault();
            if (snd is null) continue;
            map[row.Role] = snd.Id;
        }

        await _svc.SaveKitStateAsync(newId, map);

        await ReloadKitsAsync();
        SelectedKit = Kits.First(x => x.Model!.Id == newId);

        _baseKit = null;
    }

    private void RemovePatternDraftIfAny()
    {
        var draft = Patterns.FirstOrDefault(x => x.IsDraft);
        if (draft is null) return;

        Patterns.Remove(draft);
        _basePattern = null;
    }

    private void RemoveKitDraftIfAny()
    {
        var draft = Kits.FirstOrDefault(x => x.IsDraft);
        if (draft is null) return;

        Kits.Remove(draft);
        _baseKit = null;
    }

    private async Task DeletePatternAsync()
    {
        if (SelectedPattern is null) return;

        if (SelectedPattern.IsDraft)
        {
            Patterns.Remove(SelectedPattern);
            SelectedPattern = _basePattern ?? Patterns.FirstOrDefault();
            _basePattern = null;
            return;
        }

        var savedCount = Patterns.Count(x => !x.IsDraft);
        if (savedCount <= 1) return;

        var id = SelectedPattern.Model!.Id;
        await _svc.DeletePatternAsync(id);

        await ReloadPatternsAsync();
        SelectedPattern = Patterns.FirstOrDefault();
    }


    private async Task DeleteKitAsync()
    {
        if (SelectedKit is null) return;

        if (SelectedKit.IsDraft)
        {
            Kits.Remove(SelectedKit);
            SelectedKit = _baseKit ?? Kits.FirstOrDefault();
            _baseKit = null;
            return;
        }

        var savedCount = Kits.Count(x => !x.IsDraft);
        if (savedCount <= 1) return;

        var id = SelectedKit.Model!.Id;
        await _svc.DeleteKitAsync(id);

        await ReloadKitsAsync();
        SelectedKit = Kits.FirstOrDefault();
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}

public sealed class RowVm : INotifyPropertyChanged
{
    public TrackRole Role { get; }
    public ObservableCollection<SoundBase> Sounds { get; }
    public ObservableCollection<StepCellVm> Steps { get; } = new();

    private bool _suppress;

    private SoundBase? _selectedSound;
    public SoundBase? SelectedSound
    {
        get => _selectedSound;
        set
        {
            if (ReferenceEquals(_selectedSound, value)) return;
            _selectedSound = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSound)));

            if (_suppress) return;
            Changed?.Invoke();
        }
    }

    public event Action? Changed;

    public RowVm(TrackRole role, List<SoundBase> sounds)
    {
        Role = role;
        Sounds = new ObservableCollection<SoundBase>(sounds);

        _suppress = true;
        _selectedSound = Sounds.FirstOrDefault();
        _suppress = false;
    }

    public void SetSelectedSoundFromDb(SoundBase? sound)
    {
        _suppress = true;
        SelectedSound = sound;
        _suppress = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class StepCellVm : INotifyPropertyChanged
{
    public TrackRole Role { get; }
    public int StepIndex { get; }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (_isOn == value) return;
            _isOn = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOn)));
        }
    }

    public StepCellVm(TrackRole role, int stepIndex, bool isOn)
    {
        Role = role;
        StepIndex = stepIndex;
        _isOn = isOn;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}

public sealed class RelayCommand<T> : ICommand where T : class
{
    private readonly Func<T?, Task> _executeAsync;
    public RelayCommand(Func<T?, Task> executeAsync) => _executeAsync = executeAsync;
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await _executeAsync(parameter as T);
    public event EventHandler? CanExecuteChanged;
}
