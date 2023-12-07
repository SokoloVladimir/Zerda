using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Назначение варианта работы конкретной группе
/// </summary>
[PrimaryKey("GroupId", "WorkId")]
[Index("GroupId", Name = "GroupId_INDEX")]
[Index("WorkId", Name = "WorkVariantId_INDEX")]
public partial class Assignment
{
    /// <summary>
    /// Идентификатор работы
    /// </summary>
    [Key]
    public int WorkId { get; set; }

    /// <summary>
    /// Идентификатор группы
    /// </summary>
    [Key]
    public int GroupId { get; set; }

    /// <summary>
    /// Дата назначения работы
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? AssignedDate { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Assignment")]
    public virtual Group Group { get; set; } = null!;

    [ForeignKey("WorkId")]
    [InverseProperty("Assignment")]
    public virtual Work Work { get; set; } = null!;
}
