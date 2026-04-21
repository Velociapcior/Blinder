using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blinder.Api.Persistence.Configurations;

internal sealed class AppSchemaMarkerConfiguration : IEntityTypeConfiguration<AppSchemaMarker>
{
    /// <summary>
    /// Maps the app boundary marker table used to bootstrap isolated migrations safely.
    /// </summary>
    public void Configure(EntityTypeBuilder<AppSchemaMarker> builder)
    {
        builder.ToTable(AppPersistenceDefaults.SchemaMarkerTable);
        builder.HasKey(marker => marker.Id);
        builder.Property(marker => marker.Id).ValueGeneratedOnAdd();
    }
}