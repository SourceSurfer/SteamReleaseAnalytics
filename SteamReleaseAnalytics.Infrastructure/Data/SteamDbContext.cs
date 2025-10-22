using Microsoft.EntityFrameworkCore;
using SteamReleaseAnalytics.Core.Models;

namespace SteamReleaseAnalytics.Infrastructure.Data
{
    public class SteamDbContext : DbContext
    {
        public SteamDbContext(DbContextOptions<SteamDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GameTag> GameTags { get; set; }
        public DbSet<GameSnapshot> GameSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация Game
            modelBuilder.Entity<Game>()
                .HasKey(g => g.SteamAppId);

            modelBuilder.Entity<Game>()
                .HasMany(g => g.GameTags)
                .WithOne(gt => gt.Game)
                .HasForeignKey(gt => gt.GameSteamAppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                .HasMany(g => g.Snapshots)
                .WithOne(s => s.Game)
                .HasForeignKey(s => s.GameSteamAppId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация GameTag
            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Game)
                .WithMany(g => g.GameTags)
                .HasForeignKey(gt => gt.GameSteamAppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameTag>()
                .HasOne(gt => gt.Tag)
                .WithMany(t => t.GameTags)
                .HasForeignKey(gt => gt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для оптимизации
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.ReleaseDate);

            modelBuilder.Entity<GameSnapshot>()
                .HasIndex(s => s.SnapshotDate);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}