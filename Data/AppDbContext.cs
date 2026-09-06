using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tutor> Tutors => Set<Tutor>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<LessonEvent> LessonEvents => Set<LessonEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tutor>(entity =>
        {
            entity.HasKey(tutor => tutor.Id);
            entity.Property(tutor => tutor.Id).HasMaxLength(16);
            entity.Property(tutor => tutor.Name).IsRequired();
            entity.Property(tutor => tutor.Subject).IsRequired();
            entity.Property(tutor => tutor.Phone).IsRequired();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(room => room.Id);
            entity.Property(room => room.Id).HasMaxLength(16);
            entity.Property(room => room.Name).IsRequired();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.Id).HasMaxLength(16);
            entity.Property(booking => booking.StudentName).IsRequired();
            entity.Property(booking => booking.LessonDate).IsRequired();
            entity.Property(booking => booking.StartTime).IsRequired();
            entity.Property(booking => booking.DurationMinutes).IsRequired();

            // Stored as text ("Booked"/"Cancelled"/"NoShow") so the column stays
            // readable and does not depend on enum ordinal values.
            entity.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(booking => booking.GroupId).HasMaxLength(16);

            entity.HasOne(booking => booking.Tutor)
                .WithMany()
                .HasForeignKey(booking => booking.TutorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(booking => booking.Room)
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Not unique on purpose: the seed export contains real historical
            // conflicts (overlapping tutor/room bookings, daily-limit breaches).
            // Those are detected in code, not blocked by the schema.
            entity.HasIndex(booking => new { booking.TutorId, booking.LessonDate });
            entity.HasIndex(booking => new { booking.RoomId, booking.LessonDate });
        });

        modelBuilder.Entity<LessonEvent>(entity =>
        {
            entity.HasKey(lessonEvent => lessonEvent.Id);

            entity.Property(lessonEvent => lessonEvent.Type)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(lessonEvent => lessonEvent.OccurredAt).IsRequired();
            entity.Property(lessonEvent => lessonEvent.FromRoomId).HasMaxLength(16);
            entity.Property(lessonEvent => lessonEvent.ToRoomId).HasMaxLength(16);
            entity.Property(lessonEvent => lessonEvent.Reason).HasMaxLength(500);

            entity.HasOne(lessonEvent => lessonEvent.Lesson)
                .WithMany()
                .HasForeignKey(lessonEvent => lessonEvent.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lessonEvent => lessonEvent.LessonId);
        });
    }
}
