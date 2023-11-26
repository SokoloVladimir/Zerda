using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Data.Model;

/// <summary>
/// Семестр академического года
/// </summary>
public partial class Semester
{
    /// <summary>
    /// Идентификатор семестра
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Первый календарный год
    /// </summary>
    [Column(TypeName = "year")]
    public short StartYear { get; set; }

    /// <summary>
    /// Если второй семестр
    /// </summary>
    public bool IsSecond { get; set; }

    [JsonIgnore]
    [InverseProperty("Semester")]
    public virtual ICollection<Work> Work { get; } = new List<Work>();
}
