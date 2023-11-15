using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

[Index("Name", Name = "Name_UNIQUE", IsUnique = true)]
public partial class WorkType
{
    /// <summary>
    /// Идентифицирующее наименование типа работы
    /// </summary>
    [Key]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("WorkTypeNameNavigation")]
    public virtual ICollection<Work> Work { get; } = new List<Work>();
}
