﻿namespace CityInfo.API.Models;
/// <summary>
/// A city without points of interest
/// </summary>
public class CityWithoutPointsOfInterestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
