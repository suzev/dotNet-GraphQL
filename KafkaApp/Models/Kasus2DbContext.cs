using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace KafkaApp.Models
{
    public partial class Kasus2DbContext : DbContext
    {
        public Kasus2DbContext()
        {
        }

        public Kasus2DbContext(DbContextOptions<Kasus2DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Twittor> Twittors { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();
            optionsBuilder.UseSqlServer(config["Settings:Database"]);
            /* if (!optionsBuilder.IsConfigured)
             {
 #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                 optionsBuilder.UseSqlServer("Server=LAPTOP-9NEN6LM2\\SQLEXPRESS;Database=Kasus2Db;Trusted_Connection=True;");
             }
            */
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.Comment1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Comment");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Updated).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Comment_User");
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.ToTable("Profile");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Profiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Profile_User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Twittor>(entity =>
            {
                entity.ToTable("Twittor");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Post)
                    .IsRequired()
                    .HasMaxLength(144)
                    .IsUnicode(false);

                entity.Property(e => e.Updated).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Twittors)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Twittor_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Id, "IX_Unique_Username")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.Updated).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRole");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
