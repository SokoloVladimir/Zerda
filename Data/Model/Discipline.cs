using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[Index("Name", Name = "Name_UNIQUE", IsUnique = true)]
public partial class Discipline
{
    /// <summary>
    /// Идентификатор дисциплины
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Наименование (код + полное имя) дисциплины
    /// </summary>
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("Discipline")]
    public virtual ICollection<Work> Work { get; } = new List<Work>();
}
