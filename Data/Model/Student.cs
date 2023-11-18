using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

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
    public string Midname { get; set; } = null!;

    /// <summary>
    /// Логин аккаунта для входа
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Внешний идентификатор группы
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Включена ли тёмная тема у пользователя
    /// </summary>
    public sbyte IsDarkTheme { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Student")]
    public virtual Account? Account { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Student")]
    public virtual Group Group { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();
}
