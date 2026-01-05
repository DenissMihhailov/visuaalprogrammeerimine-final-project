using System.ComponentModel;

namespace RhythmMachineUI;

public sealed class ComboItemVm<T> : INotifyPropertyChanged where T : class
{
    public T? Model { get; }
    public bool IsDraft { get; }

    private readonly string _name;

    public string DisplayName => IsDraft ? "unsaved*" : _name;

    public ComboItemVm(T model, string name)
    {
        Model = model;
        _name = name;
        IsDraft = false;
    }

    private ComboItemVm()
    {
        Model = null;
        _name = "";
        IsDraft = true;
    }

    public static ComboItemVm<T> Draft() => new ComboItemVm<T>();

    public event PropertyChangedEventHandler? PropertyChanged;
}
