using App.Domain.Enums;
using System.Text;

namespace App.Infrastructure.Export;

public sealed class CsvExporter
{
    public void ExportPattern(string filePath, int bpm, int stepsCount, Dictionary<TrackRole, bool[]> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bpm," + bpm);
        sb.AppendLine("Role,Step,IsOn");

        foreach (var (role, steps) in data)
        {
            for (int i = 0; i < stepsCount; i++)
                sb.AppendLine($"{role},{i},{(steps[i] ? 1 : 0)}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}
