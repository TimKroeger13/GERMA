using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;


public class BoundingBoxData
{
    public TreeData? Treedata { get; set; }
}
public class TreeData
{
    public int? Srid { get; set; }
    public string? GeojsonTree { get; set; }

}