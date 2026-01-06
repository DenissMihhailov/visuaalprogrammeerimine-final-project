using App.Domain.Entities.Kits;
using App.Domain.Entities.Patterns;
using App.Domain.Entities.Sounds;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<Pattern> Patterns => Set<Pattern>();
    public DbSet<PatternStep> PatternSteps => Set<PatternStep>();
    public DbSet<Kit> Kits => Set<Kit>();
    public DbSet<SoundBase> Sounds => Set<SoundBase>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SoundBase>()
            .ToTable("Sounds")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<BuiltInSound>("BuiltIn")
            .HasValue<UserSound>("User");

        b.Entity<BuiltInSound>().Property(x => x.ResourcePath).IsRequired();
        b.Entity<UserSound>().Property(x => x.FilePath).IsRequired();

        b.Entity<Kit>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();

            e.HasMany(x => x.Slots)
             .WithOne()
             .HasForeignKey(x => x.KitId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<KitSlot>(e =>
        {
            e.ToTable("KitSlots");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.KitId, x.Role }).IsUnique();
        });

        b.Entity<Pattern>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            e.HasMany(x => x.Steps)
             .WithOne()
             .HasForeignKey(x => x.PatternId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PatternStep>(e =>
        {
            e.ToTable("PatternSteps");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PatternId, x.Role, x.StepIndex }).IsUnique();
        });

        Seed(b);
    }

    private static void Seed(ModelBuilder b)
    {
        var now = new DateTime(2026, 1, 1);

        var rockId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var emptyId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var trapId = Guid.Parse("00000000-0000-0000-0000-000000000001");


        b.Entity<Pattern>().HasData(new
        {
            Id = rockId,
            Code = "AROCK-001",
            Name = "Rock basic",
            StepsCount = 16,
            Bpm = 75,
            Status = PatternStatus.Ready,
            CreatedAt = now
        });

        b.Entity<Pattern>().HasData(new
        {
            Id = emptyId,
            Code = "EMPTY-001",
            Name = "Empty",
            StepsCount = 16,
            Bpm = 120,
            Status = PatternStatus.Draft,
            CreatedAt = now
        });

        b.Entity<Pattern>().HasData(new
        {
            Id = trapId,
            Code = "TRAP-DETROIT",
            Name = "Detroit Trap",
            StepsCount = 16,
            Bpm = 128,
            Status = PatternStatus.Ready,
            CreatedAt = now
        });

        var steps = new List<object>();
        foreach (var role in Enum.GetValues<TrackRole>())
        {
            for (int i = 0; i < 16; i++)
            {
                var on = role switch
                {
                    TrackRole.Kick => i is 0 or 3 or 10 or 12,
                    TrackRole.Snare => i is 4 or 12,
                    TrackRole.HatClosed => i is 0 or 2 or 4 or 6 or 8 or 10 or 15,
                    TrackRole.HatOpen => i is 12 or 14,
                    TrackRole.Perc => i is 0 or 8,
                    TrackRole.Crash => i is 0,
                    _ => false
                };

                steps.Add(new { Id = Guid.NewGuid(), PatternId = rockId, Role = role, StepIndex = i, IsOn = on });
            }
        }

        foreach (var role in Enum.GetValues<TrackRole>())
        {
            for (int i = 0; i < 16; i++)
            {
                steps.Add(new { Id = Guid.NewGuid(), PatternId = emptyId, Role = role, StepIndex = i, IsOn = false });
            }
        }

        foreach (var role in Enum.GetValues<TrackRole>())
        {
            for (int i = 0; i < 16; i++)
            {
                var on = role switch
                {
                    TrackRole.Kick => i is 0 or 8 or 13,
                    TrackRole.Snare => i is 4 or 9 or 12,
                    TrackRole.HatClosed => i is 0 or 2 or 4 or 6 or 8 or 9 or 10 or 12 or 14 or 15,
                    TrackRole.Perc => i is 14,
                    TrackRole.Crash => i is 0,
                    _ => false
                };

                steps.Add(new { Id = Guid.NewGuid(), PatternId = trapId, Role = role, StepIndex = i, IsOn = on });
            }
        }

        b.Entity<PatternStep>().HasData(steps.ToArray());
    }

}
