using App.Application.Services;
using App.Infrastructure.Audio;
using System.Windows;

namespace RhythmMachineUI;

public partial class MainWindow : Window
{
    public MainWindow(SequencerService svc, IAudioEngine audio)
    {
        InitializeComponent();
        DataContext = new MainVm(svc, audio);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
