using App.Application.Services;
using App.Infrastructure.Audio;
using App.Infrastructure.Export;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;


namespace RhythmMachineUI;

public partial class MainWindow : Window
{
    public MainWindow(SequencerService svc, IAudioEngine audio)
    {
        InitializeComponent();

        var csvExporter = ((App)Application.Current)
                            ._host!.Services
                            .GetRequiredService<CsvExporter>();

        DataContext = new MainVm(svc, audio, csvExporter);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
