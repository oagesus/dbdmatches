using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<MatchSurvivor> MatchSurvivors => Set<MatchSurvivor>();
    public DbSet<MatchKiller> MatchKillers => Set<MatchKiller>();
    public DbSet<PlayerStatsSnapshot> PlayerStatsSnapshots => Set<PlayerStatsSnapshot>();
    public DbSet<Streak> Streaks => Set<Streak>();
    public DbSet<StreakKiller> StreakKillers => Set<StreakKiller>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => e.SteamId).IsUnique();
            entity.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.SteamId).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.TokenHash).HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Session)
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.LoggedInAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastActivityAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchSurvivor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PlayedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Result).HasConversion<string>().HasMaxLength(10);

            entity.HasOne(e => e.User)
                .WithMany(e => e.SurvivorMatches)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchKiller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Killer).HasMaxLength(50);
            entity.Property(e => e.PlayedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Result).HasConversion<string>().HasMaxLength(10);

            entity.HasOne(e => e.User)
                .WithMany(e => e.KillerMatches)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerStatsSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.StatName }).IsUnique();
            entity.Property(e => e.StatName).HasMaxLength(100);
            entity.Property(e => e.FetchedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.User)
                .WithMany(e => e.StatsSnapshots)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Streak>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithOne(e => e.Streak)
                .HasForeignKey<Streak>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StreakKiller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Killer }).IsUnique();
            entity.Property(e => e.Killer).HasMaxLength(50);

            entity.HasOne(e => e.User)
                .WithMany(e => e.StreakKillers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
