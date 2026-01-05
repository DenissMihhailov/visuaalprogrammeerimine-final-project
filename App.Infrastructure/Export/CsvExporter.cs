using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Export;

public sealed class CsvExporter
{
    private readonly AppDbContext _db;

    public CsvExporter(AppDbContext db) => _db = db;

    public async Task ExportPatternAsync(Guid patternId, string filePath)
    {
        var p = await _db.Patterns.Include(x => x.Steps).AsNoTracking().FirstAsync(x => x.Id == patternId);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var sw = new StreamWriter(filePath);
        await sw.WriteLineAsync("Code,Name,StepsCount,BPM,Status");
        await sw.WriteLineAsync($"{Q(p.Code)},{Q(p.Name)},{p.StepsCount},{p.Bpm},{p.Status}");
        await sw.WriteLineAsync();
        await sw.WriteLineAsync("Role,StepIndex,IsOn");

        foreach (var s in p.Steps.OrderBy(x => x.Role).ThenBy(x => x.StepIndex))
            await sw.WriteLineAsync($"{s.Role},{s.StepIndex},{(s.IsOn ? 1 : 0)}");
    }

    private static string Q(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
}

