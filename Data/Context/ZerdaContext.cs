using System;
using System.Collections.Generic;
using Context.Data;
using Microsoft.EntityFrameworkCore;

namespace Context.Model;

public partial class ZerdaContext : DbContext
{
    public ZerdaContext()
    {
    }

    public ZerdaContext(DbContextOptions<ZerdaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Account { get; set; }

    public virtual DbSet<Discipline> Discipline { get; set; }

    public virtual DbSet<Group> Group { get; set; }

    public virtual DbSet<Result> Result { get; set; }

    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<Work> Work { get; set; }

    public virtual DbSet<WorkType> WorkType { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasComment("Идентификатор аккаунта");
            entity.Property(e => e.Login).HasComment("Логин аккаунта");
            entity.Property(e => e.PasswordHash)
                .IsFixedLength()
                .HasComment("Хэш пароля алгоритмом bcrypt ");
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasComment("Идентификатор дисциплины");
            entity.Property(e => e.Name).HasComment("Наименование (код + полное имя) дисциплины");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasComment("Идентификатор группы");
            entity.Property(e => e.Name)
                .IsFixedLength()
                .HasComment("Наименование (аббревиатура) группы");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.WorkId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.UserId).HasComment("Идентификатор пользователя");
            entity.Property(e => e.WorkId).HasComment("Идентификатор работы");
            entity.Property(e => e.Tasks).HasComment("Массив бит для обозначения выполненных работ");

            entity.HasOne(d => d.User).WithMany(p => p.Result)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Work_has_Student_Student1");

            entity.HasOne(d => d.Work).WithMany(p => p.Result)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Work_has_Student_Work1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasComment("Идентификатор пользователя");
            entity.Property(e => e.AccountId).HasComment("Логин аккаунта для входа");
            entity.Property(e => e.GroupId).HasComment("Внешний идентификатор группы");
            entity.Property(e => e.IsDarkTheme).HasComment("Включена ли тёмная тема у пользователя");
            entity.Property(e => e.Midname).HasComment("Отчество");
            entity.Property(e => e.Name).HasComment("Имя");
            entity.Property(e => e.Surname).HasComment("Фамилия");

            entity.HasOne(d => d.Account).WithOne(p => p.User).HasConstraintName("Account_User_FK");

            entity.HasOne(d => d.Group).WithMany(p => p.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Group_User_FK");
        });

        modelBuilder.Entity<Work>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasComment("Идентификатор работы");
            entity.Property(e => e.DateEst).HasComment("Время до которого нужно сдать работу, если NULL - бессрочно");
            entity.Property(e => e.DisciplineId).HasComment("Внешний идентификатор дисциплины");
            entity.Property(e => e.Number).HasComment("Номер работы");
            entity.Property(e => e.TaskCount)
                .HasDefaultValueSql("'1'")
                .HasComment("Количество заданий в работе, по умолчанию - 1");
            entity.Property(e => e.Theme)
                .HasDefaultValueSql("'Без темы'")
                .HasComment("Тема работы. Может быть достаточно длинным.");
            entity.Property(e => e.WorkTypeName).HasComment("Тип работы (внешний ключ)");

            entity.HasOne(d => d.Discipline).WithMany(p => p.Work)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Discipline_Work_FK");

            entity.HasOne(d => d.WorkTypeNameNavigation).WithMany(p => p.Work)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("WorkType_Work_FK");
        });

        modelBuilder.Entity<WorkType>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity.Property(e => e.Name).HasComment("Идентифицирующее наименование типа работы");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
