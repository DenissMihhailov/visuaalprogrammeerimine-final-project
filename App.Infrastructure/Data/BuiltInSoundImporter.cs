using System.Security.Cryptography;
using System.Text;
using App.Domain.Entities.Sounds;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public static class BuiltInSoundImporter
{
    public static async Task ImportAsync(AppDbContext db, string baseDir)
    {
        var root = Path.Combine(baseDir, "Assets", "Sounds");
        if (!Directory.Exists(root)) return;

        var files = Directory.EnumerateFiles(root, "*.wav", SearchOption.AllDirectories).ToList();

        var existing = await db.Sounds
            .OfType<BuiltInSound>()
            .AsNoTracking()
            .Select(x => x.ResourcePath)
            .ToListAsync();

        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var absPath in files)
        {
            var rel = "Assets/Sounds/" + Path.GetRelativePath(root, absPath).Replace('\\', '/');


            if (existingSet.Contains(rel)) continue;

            var role = InferRoleFromRelativePath(rel);
            var name = MakeNiceName(Path.GetFileNameWithoutExtension(absPath));
            var id = DeterministicGuid("builtin:" + rel);

            db.Sounds.Add(new BuiltInSound(id, name, role, rel, now));
            existingSet.Add(rel);
        }

        await db.SaveChangesAsync();
    }

    private static TrackRole InferRoleFromRelativePath(string rel)
    {
        var p = rel.Replace('\\', '/').ToLowerInvariant();

        if (p.Contains("/kick/")) return TrackRole.Kick;
        if (p.Contains("/snare/")) return TrackRole.Snare;
        if (p.Contains("/hatclosed/")) return TrackRole.HatClosed;
        if (p.Contains("/hatopen/")) return TrackRole.HatOpen;
        if (p.Contains("/perc/")) return TrackRole.Perc;
        if (p.Contains("/crash/")) return TrackRole.Crash;

        throw new InvalidOperationException($"Cannot infer role from path: {rel}");
    }

    private static string MakeNiceName(string file)
    {
        var s = file.Replace('_', ' ').Replace('-', ' ').Trim();
        return string.IsNullOrWhiteSpace(s) ? "Sound" : s;
    }

    private static Guid DeterministicGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}

