using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Назначение варианта работы конкретной группе
/// </summary>
[PrimaryKey("WorkVariantId", "GroupId")]
[Index("GroupId", Name = "GroupId_INDEX")]
[Index("WorkVariantId", Name = "WorkVariantId_INDEX")]
public partial class Assignment
{
    /// <summary>
    /// Внешний ключ варианта работы
    /// </summary>
    [Key]
    public int WorkVariantId { get; set; }

    /// <summary>
    /// Внешний ключ группы
    /// </summary>
    [Key]
    public int GroupId { get; set; }

    /// <summary>
    /// Дата назначения варианта работы
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime AssignedDate { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Assignment")]
    public virtual Group Group { get; set; } = null!;

    [ForeignKey("WorkVariantId")]
    [InverseProperty("Assignment")]
    public virtual WorkVariant WorkVariant { get; set; } = null!;
}
