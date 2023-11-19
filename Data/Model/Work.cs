using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Базовая работа
/// </summary>
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
    /// Внешний идентификатор дисциплины
    /// </summary>
    public int DisciplineId { get; set; }

    /// <summary>
    /// Тип работы (внешний ключ)
    /// </summary>
    public int WorkTypeId { get; set; }

    /// <summary>
    /// Номер работы
    /// </summary>
    public sbyte Number { get; set; }

    /// <summary>
    /// Тема работы. Может быть достаточно длинным.
    /// </summary>
    [StringLength(150)]
    public string? Theme { get; set; }

    [ForeignKey("DisciplineId")]
    [InverseProperty("Work")]
    public virtual Discipline Discipline { get; set; } = null!;

    [ForeignKey("WorkTypeId")]
    [InverseProperty("Work")]
    public virtual WorkType WorkType { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Work")]
    public virtual ICollection<WorkVariant> WorkVariant { get; } = new List<WorkVariant>();
}
