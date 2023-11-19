using System;
using System.Collections.Generic;
using Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

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

    public virtual DbSet<Assignment> Assignment { get; set; }

    public virtual DbSet<Discipline> Discipline { get; set; }

    public virtual DbSet<Group> Group { get; set; }

    public virtual DbSet<Result> Result { get; set; }

    public virtual DbSet<Student> Student { get; set; }

    public virtual DbSet<Work> Work { get; set; }

    public virtual DbSet<WorkType> WorkType { get; set; }

    public virtual DbSet<WorkVariant> WorkVariant { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Аккаунт"));

            entity.Property(e => e.Id).HasComment("Идентификатор аккаунта");
            entity.Property(e => e.Login).HasComment("Логин аккаунта");
            entity.Property(e => e.PasswordHash)
                .IsFixedLength()
                .HasComment("Хэш пароля алгоритмом bcrypt ");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => new { e.WorkVariantId, e.GroupId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable(tb => tb.HasComment("Назначение варианта работы конкретной группе"));

            entity.Property(e => e.WorkVariantId).HasComment("Внешний ключ варианта работы");
            entity.Property(e => e.GroupId).HasComment("Внешний ключ группы");
            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Дата назначения варианта работы");

            entity.HasOne(d => d.Group).WithMany(p => p.Assignment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Group_Assignment_FK");

            entity.HasOne(d => d.WorkVariant).WithMany(p => p.Assignment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("WorkVariant_Assignment_FK");
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Дисциплина"));

            entity.Property(e => e.Id).HasComment("Идентификатор дисциплины");
            entity.Property(e => e.Name).HasComment("Наименование (код + полное имя) дисциплины");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Группа"));

            entity.Property(e => e.Id).HasComment("Идентификатор группы");
            entity.Property(e => e.Name).HasComment("Наименование (аббревиатура) группы");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.WorkVariantId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable(tb => tb.HasComment("Результат выполнения варианта работы"));

            entity.Property(e => e.StudentId).HasComment("Идентификатор пользователя");
            entity.Property(e => e.WorkVariantId).HasComment("Идентификатор работы");
            entity.Property(e => e.LastEdit).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Tasks)
                .HasDefaultValueSql("b'0'")
                .HasComment("Массив бит для обозначения выполненных работ");

            entity.HasOne(d => d.Student).WithMany(p => p.Result).HasConstraintName("Student_Result_FK");

            entity.HasOne(d => d.WorkVariant).WithMany(p => p.Result).HasConstraintName("WorkVariant_Result_FK");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Студент"));

            entity.Property(e => e.Id).HasComment("Идентификатор пользователя");
            entity.Property(e => e.AccountId).HasComment("Логин аккаунта для входа");
            entity.Property(e => e.GroupId).HasComment("Внешний идентификатор группы");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("b'0'");
            entity.Property(e => e.Name).HasComment("Имя");
            entity.Property(e => e.Patronym).HasComment("Отчество");
            entity.Property(e => e.Surname).HasComment("Фамилия");

            entity.HasOne(d => d.Account).WithOne(p => p.Student).HasConstraintName("Account_Student_FK");

            entity.HasOne(d => d.Group).WithMany(p => p.Student)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Group_Student_FK");
        });

        modelBuilder.Entity<Work>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Базовая работа"));

            entity.Property(e => e.Id).HasComment("Идентификатор работы");
            entity.Property(e => e.DisciplineId).HasComment("Внешний идентификатор дисциплины");
            entity.Property(e => e.Number).HasComment("Номер работы");
            entity.Property(e => e.Theme)
                .HasDefaultValueSql("'Без темы'")
                .HasComment("Тема работы. Может быть достаточно длинным.");
            entity.Property(e => e.WorkTypeId).HasComment("Тип работы (внешний ключ)");

            entity.HasOne(d => d.Discipline).WithMany(p => p.Work)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Discipline_Work_FK");

            entity.HasOne(d => d.WorkType).WithMany(p => p.Work)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("WorkType_Work_FK");
        });

        modelBuilder.Entity<WorkType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Тип работы"));

            entity.Property(e => e.Id).HasComment("Идентификатор типа работы");
            entity.Property(e => e.Name).HasComment("Иаименование типа работы");
        });

        modelBuilder.Entity<WorkVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Вариант работы"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("Идентификатор варианта");
            entity.Property(e => e.TaskCount)
                .HasDefaultValueSql("'5'")
                .HasComment("Количество заданий в варианте");
            entity.Property(e => e.TasksFor3).HasComment("Количество выполненных заданий необходимых для оценки 3");
            entity.Property(e => e.TasksFor4).HasComment("Количество выполненных заданий необходимых для оценки 4");
            entity.Property(e => e.TasksFor5).HasComment("Количество выполненных заданий необходимых для оценки 5");
            entity.Property(e => e.WorkId).HasComment("Внешний ключ родительской работы");

            entity.HasOne(d => d.Work).WithMany(p => p.WorkVariant)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Work_WorkVariant_FK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
