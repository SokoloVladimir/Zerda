using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Студент
/// </summary>
[Index("AccountId", Name = "AccountId_UNIQUE", IsUnique = true)]
[Index("GroupId", Name = "Group_FK")]
public partial class Student
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    [StringLength(50)]
    public string Surname { get; set; } = null!;

    /// <summary>
    /// Имя
    /// </summary>
    [StringLength(50)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Отчество
    /// </summary>
    [StringLength(50)]
    public string? Patronym { get; set; }

    /// <summary>
    /// Логин аккаунта для входа
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Внешний идентификатор группы
    /// </summary>
    public int GroupId { get; set; }

    [Column(TypeName = "bit(1)")]
    public ulong IsDeleted { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Student")]
    public virtual Account? Account { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Student")]
    public virtual Group Group { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Student")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();
}
