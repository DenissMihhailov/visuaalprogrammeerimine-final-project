using System.Windows;

namespace RhythmMachineUI;

public partial class NameDialog : Window
{
    public string Prompt { get; }
    public string Name { get; set; } = "";

    public NameDialog(string prompt)
    {
        InitializeComponent();
        Prompt = prompt;
        DataContext = this;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var text = NameBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(text))
        {
            MessageBox.Show("Enter a name");
            return;
        }

        Name = text;
        DialogResult = true;
    }



}
