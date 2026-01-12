using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class Crs
{
    public string? Type { get; set; }
    public Properties? Properties { get; set; }
}
public class Feature
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public Geometry? Geometry { get; set; }
    public Properties? Properties { get; set; }
    public string? Geometry_name { get; set; }
    public List<double>? Bbox { get; set; }
}
public class Geometry
{
    public string? Type { get; set; }
    public List<double>? CoordinatesSingle { get; set; }
    public List<List<double>>? CoordinatesShort { get; set; }
    public List<List<List<double>>>? Coordinates { get; set; }
    public List<List<List<List<double>>>>? CoordinatesLong { get; set; }
}
public class Properties
{
    public int Id { get; set; }
    public string? Gridcode { get; set; }
    public string? La_40txt { get; set; }
    public string? La_60txt { get; set; }
    public string? La_80txt { get; set; }
    public string? La_100txt { get; set; }
    public string? La_100xt { get; set; }
    public string? Name { get; set; }
    public string? La_100 { get; set; }
    public string? Text { get; set; }
    public string? Bezeich { get; set; }
    public string? Afl { get; set; }
    public string? Fsko { get; set; }
    public string? Zae { get; set; }
    public string? Nen { get; set; }
    public string? Gmk { get; set; }
    public string? Namgmk { get; set; }
    public string? Fln { get; set; }
    public string? Gdz { get; set; }
    public string? Namgem { get; set; }
    public string? Zde { get; set; }
    public string? Dst { get; set; }
    public DateTime? Beg { get; set; }
    public DateTime? Statusdat { get; set; }
    public string? Uuid { get; set; }
    public int? Importid { get; set; }
    //public string? Hoehe { get; set; }

    public int? Import_id { get; set; }
    public double? Hoehe { get; set; }
    public int? Hoehe_b { get; set; }
    public int? Stift { get; set; }
    public int? Strichart { get; set; }

    public string? Herkunft { get; set; }
    public string? Name_karte { get; set; }
    public string? Nh4 { get; set; }
    public string? Lf { get; set; }
    public string? Cl { get; set; }
    public string? Po4 { get; set; }
    public string? So4 { get; set; }
    public string? K { get; set; }
    public string? B { get; set; }
    public string? Csv { get; set; }
    public string? Point_x { get; set; }
    public string? Point_y { get; set; }
    public string? Invhyas { get; set; }
    public string? Invname { get; set; }
    public string? Eigentuemer { get; set; }
    public string? Messnetz_klartext { get; set; }
    public string? Zcoordb { get; set; }
    public string? Invmbeg { get; set; }
    public string? Invzbeg { get; set; }
    public string? Invzend { get; set; }
    public string? Xcoord { get; set; }
    public string? Ycoord { get; set; }
    public string? Grwtemp_text { get; set; }
    public string? Grwtemp { get; set; }
    public string? Wasserwerk { get; set; }
    public string? Gebietsnr { get; set; }
    public string? Zone { get; set; }
    public string? Verordnung { get; set; }
    public string? Datum { get; set; }
    public string? Gvbl { get; set; }
    public string? Veror_link { get; set; }
    public string? Ae_datum { get; set; }
    public string? Ae_gvbl { get; set; }
    public string? Zehgw_m_tx { get; set; }
    public string? Gfk { get; set; }
    public string? Bezgfk { get; set; }
    public string? Ofl { get; set; }
    public string? Bezofl { get; set; }
    public string? Aog { get; set; }
    public string? Aug { get; set; }
    public string? Hoh { get; set; }
    public string? Bat { get; set; }
    public string? Bezbat { get; set; }
    public string? Nam { get; set; }
    public string? Baw { get; set; }
    public string? Bezbaw { get; set; }
    public string? Zus { get; set; }
    public string? Bezzus { get; set; }
    public string? Gkn { get; set; }
    public string? Des { get; set; }
    public string? Bezdes { get; set; }
    public string? Lag { get; set; }
    public string? Namlag { get; set; }
    public string? Hnr { get; set; }
    public string? Pnr { get; set; }
    public string? Lnr { get; set; }
    public double? VALUE { get; set; }
    public string? Bezfkt { get; set; }
    public string? Veg { get; set; }
    public string? Bezveg { get; set; }
    public string? Agt { get; set; }
    public string? Bezagt { get; set; }
    public string? Lgt { get; set; }
    public string? Bezlgt { get; set; }
    public string? Hyd { get; set; }
    public string? Bezhyd { get; set; }
    public string? Art { get; set; }
    public string? Bezart { get; set; }
    public string? Bkt { get; set; }
    public string? Bezbkt { get; set; }
    public double? Rup_d { get; set; }
    public string? Schluessel { get; set; }
    public string? Grwtemp_etext { get; set; }
    public double? Gok { get; set; }
    public double? Holocene_gok { get; set; }
    public double? Saale_gok { get; set; }
    public double? Holstein_gok { get; set; }
    public double? Elster_gok { get; set; }
    public double? Miocene_gok { get; set; }
    public double? Cottbus_gok { get; set; }
    public double? Rupel_gok { get; set; }
    public double? Endteufe { get; set; }
    public string? Bohrverfahren { get; set; }
    public string? Profil { get; set; }
    public string? Profil_lang { get; set; }
    public string? Link { get; set; }
    public string? Layer  { get; set; }
}
public class Root
{
    public string? Type { get; set; }
    public List<double>? Bbox { get; set; }
    public int? TotalFeatures { get; set; }
    public List<Feature>? Features { get; set; }
    public Crs? Crs { get; set; }
    public int? NumberMatched { get; set; }
    public int? NumberReturned { get; set; }
    public DateTime? TimeStamp { get; set; }
}
