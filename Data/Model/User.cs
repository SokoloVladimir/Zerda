﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Context.Data;

[Index("AccountId", Name = "AccountId_UNIQUE", IsUnique = true)]
[Index("GroupId", Name = "Group_FK")]
public partial class User
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
    [InverseProperty("User")]
    public virtual Account? Account { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("User")]
    public virtual Group Group { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Result> Result { get; } = new List<Result>();
}
