using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

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
    public int Number { get; set; }

    /// <summary>
    /// Тема работы. Может быть достаточно длинным.
    /// </summary>
    [StringLength(150)]
    public string? Theme { get; set; }

    /// <summary>
    /// Время до которого нужно сдать работу, если NULL - бессрочно
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? DateEst { get; set; }

    /// <summary>
    /// Количество заданий в работе, по умолчанию - 1
    /// </summary>
    public int TaskCount { get; set; }

    [ForeignKey("DisciplineId")]
    [InverseProperty("Work")]
    public virtual Discipline Discipline { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Work")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();

    [ForeignKey("WorkTypeId")]
    [InverseProperty("Work")]
    public virtual WorkType WorkType { get; set; } = null!;
}
