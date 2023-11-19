using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Тип работы
/// </summary>
[Index("Name", Name = "Name_UNIQUE", IsUnique = true)]
public partial class WorkType
{
    /// <summary>
    /// Идентификатор типа работы
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Наименование типа работы
    /// </summary>
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("WorkType")]
    public virtual ICollection<Work> Work { get; } = new List<Work>();
}
