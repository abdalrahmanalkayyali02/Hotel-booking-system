using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using HBS.API.Shared.Models;
using System.Linq.Expressions;

namespace HBS.API.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<RoleTranslation> RoleTranslations { get; set; }
    public DbSet<Permissions> Permissions { get; set; }
    public DbSet<PermissionsTranslation> PermissionsTranslations { get; set; }
    public DbSet<RolePermissions> RolePermissions { get; set; }
    public DbSet<Otp> Otp { get; set; }
    public DbSet<Regions> Regions { get; set; }
    public DbSet<RegionsTranslation> RegionsTranslations { get; set; }
    public DbSet<SubRegions> SubRegions { get; set; }
    public DbSet<SubRegionsTranslation> SubRegionsTranslations { get; set; }
    public DbSet<Countries> Countries { get; set; }
    public DbSet<CountriesTranslation> CountriesTranslations { get; set; }
    public DbSet<States> States { get; set; }
    public DbSet<StatesTranslation> StatesTranslations { get; set; }
    public DbSet<Cities> Cities { get; set; }
    public DbSet<CitiesTranslation> CitiesTranslations { get; set; }
    public DbSet<Languages> Languages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseAuditLogModel).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }
            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            var property = Expression.Property(parameter, nameof(BaseAuditLogModel.IsDeleted));

            var condition = Expression.Equal(property, Expression.Constant(false));

            var lambda = Expression.Lambda(condition, parameter);

            entityType.SetQueryFilter(lambda);
        }
    }
}
