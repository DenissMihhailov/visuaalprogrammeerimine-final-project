using App.Application.Services;
using App.Domain.Entities.Kits;
using App.Domain.Entities.Sounds;
using App.Domain.Enums;
using App.Infrastructure.Audio;
using App.Infrastructure.Data;
using App.Infrastructure.Export;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Windows;

namespace RhythmMachineUI;

public partial class App : Application
{
    public IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlite($"Data Source={System.IO.Path.Combine(AppContext.BaseDirectory, "app.db")}"));

                services.AddSingleton<IAudioEngine, NaudioEngine>();
                services.AddSingleton<CsvExporter>();
                services.AddSingleton<SequencerService>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            BuiltInSoundImporter
                .ImportAsync(db, AppContext.BaseDirectory)
                .GetAwaiter()
                .GetResult();

            void EnsureKit(string kitName, Dictionary<TrackRole, string> defaults)
            {
                var kit = db.Kits
                    .Include(k => k.Slots)
                    .FirstOrDefault(k => k.Name == kitName);

                if (kit is null)
                {
                    kit = new Kit(Guid.NewGuid(), kitName, DateTime.UtcNow);
                    db.Kits.Add(kit);
                    db.SaveChanges();
                }

                var existingRoles = db.Set<KitSlot>()
                    .Where(s => s.KitId == kit.Id)
                    .Select(s => s.Role)
                    .ToHashSet();

                foreach (var role in Enum.GetValues<TrackRole>())
                {
                    if (!existingRoles.Contains(role))
                        db.Add(new KitSlot(Guid.NewGuid(), kit.Id, role, null));
                }

                db.SaveChanges();

                kit = db.Kits
                    .Include(k => k.Slots)
                    .First(k => k.Id == kit.Id);

                foreach (var (role, path) in defaults)
                {
                    var slot = kit.Slots.First(s => s.Role == role);

                    if (slot.SoundId is not null)
                        continue;

                    var normalizedPath = path.Replace('\\', '/').ToLower();

                    var sound = db.Sounds
                        .OfType<BuiltInSound>()
                        .AsNoTracking()
                        .FirstOrDefault(s => s.ResourcePath.ToLower() == normalizedPath);

                    if (sound is not null)
                        slot.SetSound(sound.Id);
                }

                db.SaveChanges();
            }

            EnsureKit("Rock Sounds", new Dictionary<TrackRole, string>
            {
                [TrackRole.Kick] = "Assets/Sounds/Kick/kick-acoustic.wav",
                [TrackRole.Snare] = "Assets/Sounds/Snare/snare-acoustic.wav",
                [TrackRole.HatClosed] = "Assets/Sounds/HatClosed/close-hihat-2.wav",
                [TrackRole.HatOpen] = "Assets/Sounds/HatOpen/open-hihat-2.wav",
                [TrackRole.Perc] = "Assets/Sounds/Perc/perc-screech.wav",
                [TrackRole.Crash] = "Assets/Sounds/Crash/crash.wav",
            });
            EnsureKit("Trap Dirty Sounds", new Dictionary<TrackRole, string>
            {
                [TrackRole.Kick] = "Assets/Sounds/Kick/kick.wav",
                [TrackRole.Snare] = "Assets/Sounds/Snare/detroit-snare.wav",
                [TrackRole.HatClosed] = "Assets/Sounds/HatClosed/trap-hi-hat.wav",
                [TrackRole.HatOpen] = "Assets/Sounds/HatOpen/open-hihat.wav",
                [TrackRole.Perc] = "Assets/Sounds/Perc/trap-perc.wav",
                [TrackRole.Crash] = "Assets/Sounds/Crash/detroit-bell.wav",
            });

        }

        base.OnStartup(e);
        _host.Services.GetRequiredService<MainWindow>().Show();
    }
}
