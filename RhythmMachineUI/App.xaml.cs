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

public partial class App : System.Windows.Application
{
    private IHost? _host;

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

            BuiltInSoundImporter.ImportAsync(db, AppContext.BaseDirectory).GetAwaiter().GetResult();

            var kit = db.Kits.Include(k => k.Slots).FirstOrDefault(k => k.Name == "Default Kit");
            if (kit is null)
            {
                kit = new Kit(Guid.NewGuid(), "Default Kit", DateTime.UtcNow);
                db.Kits.Add(kit);
                db.SaveChanges();

                foreach (var role in Enum.GetValues<TrackRole>())
                {
                    var exists = db.Set<KitSlot>().Any(s => s.KitId == kit.Id && s.Role == role);
                    if (!exists)
                        db.Add(new KitSlot(Guid.NewGuid(), kit.Id, role, null));
                }
                db.SaveChanges();

                kit = db.Kits.Include(k => k.Slots).First(k => k.Id == kit.Id);
            }

            foreach (var role in Enum.GetValues<TrackRole>())
            {
                var slot = kit.Slots.First(s => s.Role == role);

                if (slot.SoundId is null)
                {
                    var sound = db.Sounds.OfType<BuiltInSound>()
                        .AsNoTracking()
                        .Where(s => s.Role == role)
                        .OrderBy(s => s.Name)
                        .FirstOrDefault();

                    if (sound is not null)
                        slot.SetSound(sound.Id);
                }
            }

            db.SaveChanges();
        }

        base.OnStartup(e);
        _host.Services.GetRequiredService<MainWindow>().Show();
    }
}
