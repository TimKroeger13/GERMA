using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GERMAG.Shared;


public class VectorDataStreamRequest
{
    public int Srid { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MinLat must be positive")]
    public double MinLat { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MinLng must be positive")]
    public double MinLng { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxLat must be positive")]
    public double MaxLat { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxLng must be positive")]
    public double MaxLng { get; set; }
}