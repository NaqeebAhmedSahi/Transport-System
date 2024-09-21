using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Driver
{
    public int DriverId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string NumberPlate { get; set; } = null!;

    public virtual ICollection<VehicleInfo> VehicleInfos { get; set; } = new List<VehicleInfo>();
}
