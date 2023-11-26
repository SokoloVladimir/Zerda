using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Базовая работа
/// </summary>
[Index("SemesterId", Name = "Semester_Work_FK_idx")]
[Index("DisciplineId", "WorkTypeId", "Number", Name = "WorkMeta_UNIQUE", IsUnique = true)]
[Index("WorkTypeId", Name = "WorkType_INDEX")]
public partial class Work
{
    /// <summary>
    /// Идентификатор работы
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Дисциплина
    /// </summary>
    public int DisciplineId { get; set; }

    /// <summary>
    /// Тип работы 
    /// </summary>
    public int WorkTypeId { get; set; }

    /// <summary>
    /// Семестр
    /// </summary>
    public int SemesterId { get; set; }

    /// <summary>
    /// Номер работы
    /// </summary>
    public sbyte Number { get; set; }

    /// <summary>
    /// Тема работы. Может быть достаточно длинным.
    /// </summary>
    [StringLength(150)]
    public string? Theme { get; set; }

    /// <summary>
    /// Количество заданий в работе
    /// </summary>
    public sbyte TaskCount { get; set; }

    /// <summary>
    /// Количество выполенных заданий на оценку 3
    /// </summary>
    public sbyte TaskFor3 { get; set; }

    /// <summary>
    /// Количество выполенных заданий на оценку 4
    /// </summary>
    public sbyte TaskFor4 { get; set; }

    /// <summary>
    /// Количество выполенных заданий на оценку 5
    /// </summary>
    public sbyte TaskFor5 { get; set; }

    /// <summary>
    /// Документ содержащий файл работы
    /// </summary>
    [StringLength(255)]
    public string? Document { get; set; }

    [JsonIgnore]
    [InverseProperty("Work")]
    public virtual ICollection<Assignment> Assignment { get; } = new List<Assignment>();

    [ForeignKey("DisciplineId")]
    [InverseProperty("Work")]
    public virtual Discipline? Discipline { get; set; }

    [JsonIgnore]
    [InverseProperty("Work")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();

    [ForeignKey("SemesterId")]
    [InverseProperty("Work")]
    public virtual Semester? Semester { get; set; }

    [ForeignKey("WorkTypeId")]
    [InverseProperty("Work")]
    public virtual WorkType? WorkType { get; set; }
}
