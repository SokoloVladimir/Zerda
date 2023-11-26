using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Результат выполнения работы
/// </summary>
[PrimaryKey("StudentId", "WorkId")]
[Index("StudentId", Name = "StudentId_INDEX")]
[Index("WorkId", Name = "WorkId_INDEX")]
public partial class Result
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Key]
    public int StudentId { get; set; }

    /// <summary>
    /// Идентификатор работы
    /// </summary>
    [Key]
    public int WorkId { get; set; }

    /// <summary>
    /// Массив бит для обозначения выполненных работ
    /// </summary>
    [Column(TypeName = "bit(64)")]
    public ulong Tasks { get; set; }

    /// <summary>
    /// Дата последнего обновления записи
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime LastEdit { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Result")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("WorkId")]
    [InverseProperty("Result")]
    public virtual Work Work { get; set; } = null!;
}
