using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

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

    [JsonIgnore]
    [InverseProperty("Group")]
    public virtual ICollection<Student> Student { get; } = new List<Student>();
}
