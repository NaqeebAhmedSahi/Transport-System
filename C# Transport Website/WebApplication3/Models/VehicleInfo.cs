using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class VehicleInfo
{
    public int VehicleId { get; set; }

    public string NumberPlate { get; set; } = null!;

    public string InOrOut { get; set; } = null!;

    public DateTime DateTime { get; set; }

    public virtual Driver NumberPlateNavigation { get; set; } = null!;
}
