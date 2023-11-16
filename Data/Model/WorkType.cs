using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[Index("Name", Name = "Name_UNIQUE", IsUnique = true)]
public partial class WorkType
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Идентифицирующее наименование типа работы
    /// </summary>
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    [InverseProperty("WorkType")]
    public virtual ICollection<Work> Work { get; } = new List<Work>();
}
