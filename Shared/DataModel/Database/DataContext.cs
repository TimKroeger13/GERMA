using Microsoft.EntityFrameworkCore;

namespace GERMAG.DataModel.Database;

public partial class DataContext : DbContext
{
    public virtual DbSet<FingerPrint> FingerPrints { get; set; }
    public virtual DbSet<GeoDatum> GeoData { get; set; }
    public virtual DbSet<GeothermalParameter> GeothermalParameter { get; set; }
    public virtual DbSet<RequestLessThanOneDay> RequestLessThanOneDays { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("area", new[] { "berlin" })
            .HasPostgresEnum("finger_print_types", new[] { "session_loaded", "session_ended", "initial_request", "report_request" })
            .HasPostgresEnum("geometry_type", new[] { "point", "polygon", "polyline", "raster", "multipolygon" })
            .HasPostgresEnum("range", new[] { "near_range", "far_range" })
            .HasPostgresEnum("service", new[] { "restrictive", "efficiency" })
            .HasPostgresEnum("typeofdata", new[] { "land_parcels", "dgm", "geo_poten_restrict", "veg_height", "main_water_lines", "groundwater_surface_distance", "ground_water_height_main", "ground_water_height_tension", "water_ammonium", "water_bor", "water_chlor", "water_kalium", "water_sulfat", "water_ortho_phosphat", "electrical_con", "mean_water_temp_20to100", "mean_water_temp_20", "mean_water_temp_40", "mean_water_temp_60", "geodrilling_data", "geological_sections", "geo_drawing", "water_protec_areas", "expe_max_groundwater_hight", "geo_poten_100m_with_2400ha", "geo_poten_100m_with_1800ha", "geo_poten_80m_with_2400ha", "geo_poten_80m_with_1800ha", "geo_poten_60m_with_2400ha", "geo_poten_60m_with_1800ha", "geo_poten_40m_with_2400ha", "geo_poten_40m_with_1800ha", "thermal_con_40", "thermal_con_60", "thermal_con_80", "thermal_con_100", "groundwater_measuring_points", "building_surfaces", "depth_restrictions", "area_usage", "tree_points", "mean_water_temp_80", "depth_restrictions_rup", "tree_vector", "holstein_restrictions", "geologic_sections_berlin", "protection_area" })
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("tiger", "postgis_tiger_geocoder")
            .HasPostgresExtension("topology", "postgis_topology");

        modelBuilder.Entity<FingerPrint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("finger_print_pkey");

            entity.ToTable("finger_print");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Geom).HasColumnName("geom");
            entity.Property(e => e.Time)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<GeoDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("geo_data_pkey");

            entity.ToTable("geo_data");

            entity.HasIndex(e => e.Geom, "geometry_index").HasMethod("gist");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Geom).HasColumnName("geom");
            entity.Property(e => e.Parameter)
                .HasColumnType("json")
                .HasColumnName("parameter");
            entity.Property(e => e.ParameterKey).HasColumnName("parameter_key");

            entity.HasOne(d => d.ParameterKeyNavigation).WithMany(p => p.GeoData)
                .HasForeignKey(d => d.ParameterKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parameter_key");
        });

        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("geothermal_parameter_pkey");

            entity.ToTable("geothermal_parameter");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Getrequest).HasColumnName("getrequest");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.LastPing)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_ping");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_update");
            entity.Property(e => e.Srid).HasColumnName("srid");
        });

        modelBuilder.Entity<RequestLessThanOneDay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("request_less_than_one_day");

            entity.Property(e => e.StTransform).HasColumnName("st_transform");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
