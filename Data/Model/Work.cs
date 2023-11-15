using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[Index("DisciplineId", "WorkTypeName", "Number", Name = "WorkMeta_UNIQUE", IsUnique = true)]
[Index("WorkTypeName", Name = "WorkType_Work_FK_idx")]
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
    [StringLength(50)]
    public string WorkTypeName { get; set; } = null!;

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

    [InverseProperty("Work")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();

    [ForeignKey("WorkTypeName")]
    [InverseProperty("Work")]
    public virtual WorkType WorkTypeNameNavigation { get; set; } = null!;
}
