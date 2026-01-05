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

        var pId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        b.Entity<Pattern>().HasData(new
        {
            Id = pId,
            Code = "HOUSE-001",
            Name = "House Basic",
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
                    TrackRole.Kick => i is 0 or 4 or 8 or 12,
                    TrackRole.Snare => i is 4 or 12,
                    TrackRole.HatClosed => i is 2 or 6 or 10 or 14,
                    TrackRole.HatOpen => i is 14,
                    _ => false
                };

                steps.Add(new { Id = Guid.NewGuid(), PatternId = pId, Role = role, StepIndex = i, IsOn = on });
            }
        }

        b.Entity<PatternStep>().HasData(steps.ToArray());
    }
}
