using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace bike_project.Models;

public partial class Stock
{
    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
