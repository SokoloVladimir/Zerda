using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Результат выполнения варианта работы
/// </summary>
[PrimaryKey("StudentId", "WorkVariantId")]
[Index("StudentId", Name = "StudentId_INDEX")]
[Index("WorkVariantId", Name = "WorkVariant_Result_FK_idx")]
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
    public int WorkVariantId { get; set; }

    /// <summary>
    /// Массив бит для обозначения выполненных работ
    /// </summary>
    [Column(TypeName = "bit(64)")]
    public ulong Tasks { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LastEdit { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Result")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("WorkVariantId")]
    [InverseProperty("Result")]
    public virtual WorkVariant WorkVariant { get; set; } = null!;
}
