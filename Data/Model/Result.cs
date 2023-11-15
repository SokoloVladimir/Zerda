using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[PrimaryKey("UserId", "WorkId")]
[Index("UserId", Name = "fk_Work_has_Student_Student1_idx")]
[Index("WorkId", Name = "fk_Work_has_Student_Work1_idx")]
public partial class Result
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Key]
    public int UserId { get; set; }

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

    [ForeignKey("UserId")]
    [InverseProperty("Result")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("WorkId")]
    [InverseProperty("Result")]
    public virtual Work Work { get; set; } = null!;
}
