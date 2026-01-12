using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GERMAG.DataModel.Database;

public static class NpgsqlDataSourceBuilderExtensions
{
    public static NpgsqlDataSource ConfigureAndBuild(this NpgsqlDataSourceBuilder builder)
    {
        builder.EnableDynamicJson();
        builder.EnableUnmappedTypes();
        builder.MapEnum<TypeOfData>("typeofdata");
        builder.MapEnum<Area>("area");
        builder.MapEnum<Range>("range");
        builder.MapEnum<Service>("service");
        builder.MapEnum<Geometry_Type>("geometry_type");
        builder.MapEnum<FingerPrintTypes>("finger_print_types");
        builder.UseNetTopologySuite();
        //builder.MapEnum<DeliveryType>("delivery_type");
        return builder.Build();
    }
}

public partial class DataContext : DbContext
{
    private readonly string? _databaseConnection;

    public DataContext(string databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_databaseConnection ?? throw new Exception("Databaseconnection was null"));
            var dataSource = dataSourceBuilder.ConfigureAndBuild();
            optionsBuilder.UseNpgsql(dataSource, o =>
            {
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                o.UseNetTopologySuite();
            });
        }
        base.OnConfiguring(optionsBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.Property(p => p.Type).HasConversion<string>().HasColumnName("typeofdata");
            entity.Property(p => p.Area).HasConversion<string>().HasColumnName("area");
            entity.Property(p => p.Range).HasConversion<string>().HasColumnName("range");
            entity.Property(p => p.Service).HasConversion<string>().HasColumnName("service");
            entity.Property(p => p.Geometry_Type).HasConversion<string>().HasColumnName("geometry_type");
        });

        modelBuilder.Entity<FingerPrint>(entity =>
        {
            entity.Property(e => e.FingerPrintTypes).HasColumnName("finger_print_types"); //finger_print_types //FingerPrintType
        });
    }
}