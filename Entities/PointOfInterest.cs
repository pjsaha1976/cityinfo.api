﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Entities;

public class PointOfInterest
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(250)]
    public string? Description { get; set; }
    [ForeignKey("CityId")]
    public City? City { get; set; }

    public int CityId { get; set; }

    public PointOfInterest(string name)
    {
        Name = name;
    }
}
