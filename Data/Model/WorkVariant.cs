using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Вариант работы
/// </summary>
[Index("WorkId", Name = "Work_WorkVariant_idx")]
public partial class WorkVariant
{
    /// <summary>
    /// Идентификатор варианта
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Внешний ключ родительской работы
    /// </summary>
    public int WorkId { get; set; }

    /// <summary>
    /// Количество заданий в варианте
    /// </summary>
    public sbyte TaskCount { get; set; }

    /// <summary>
    /// Количество выполненных заданий необходимых для оценки 3
    /// </summary>
    public sbyte TasksFor3 { get; set; }

    /// <summary>
    /// Количество выполненных заданий необходимых для оценки 4
    /// </summary>
    public sbyte TasksFor4 { get; set; }

    /// <summary>
    /// Количество выполненных заданий необходимых для оценки 5
    /// </summary>
    public sbyte TasksFor5 { get; set; }

    [JsonIgnore]
    [InverseProperty("WorkVariant")]
    public virtual ICollection<Assignment> Assignment { get; } = new List<Assignment>();

    [JsonIgnore]
    [InverseProperty("WorkVariant")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();

    [ForeignKey("WorkId")]
    [InverseProperty("WorkVariant")]
    public virtual Work Work { get; set; } = null!;
}
