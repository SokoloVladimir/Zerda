using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[Index("Login", Name = "Login_UNIQUE", IsUnique = true)]
public partial class Account
{
    /// <summary>
    /// Идентификатор аккаунта
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Логин аккаунта
    /// </summary>
    [StringLength(25)]
    public string Login { get; set; } = null!;

    /// <summary>
    /// Хэш пароля алгоритмом bcrypt 
    /// </summary>
    [JsonIgnore]
    [StringLength(60)]
    public string PasswordHash { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Account")]
    public virtual User? User { get; set; }
}
