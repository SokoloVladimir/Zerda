using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Context.Data;

[Index("Name", Name = "Name_UNIQUE", IsUnique = true)]
public partial class Group
{
    /// <summary>
    /// Идентификатор группы
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Наименование (аббревиатура) группы
    /// </summary>
    [StringLength(10)]
    public string? Name { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<User> User { get; } = new List<User>();
}
