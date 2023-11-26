using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Группа
/// </summary>
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
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Group")]
    public virtual ICollection<Assignment> Assignment { get; } = new List<Assignment>();

    [JsonIgnore]
    [InverseProperty("Group")]
    public virtual ICollection<Student> Student { get; } = new List<Student>();
}
