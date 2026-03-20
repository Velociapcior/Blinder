using Blinder.Api.Infrastructure.Data;
using Blinder.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blinder.Tests;

public class AppDbContextTests
{
    [Fact]
    public void BuildModel_UsesSnakeCaseIdentityTablesAndIndexes()
    {
        using var context = CreateContext();

        var userEntity = context.Model.FindEntityType(typeof(ApplicationUser));
        var roleEntity = context.Model.FindEntityType(typeof(IdentityRole<Guid>));

        userEntity.Should().NotBeNull();
        roleEntity.Should().NotBeNull();

        userEntity!.GetTableName().Should().Be("asp_net_users");
        roleEntity!.GetTableName().Should().Be("asp_net_roles");
        userEntity.GetIndexes().Select(index => index.GetDatabaseName()).Should().Contain("email_index");
        userEntity.GetIndexes().Select(index => index.GetDatabaseName()).Should().Contain("user_name_index");
        userEntity.GetIndexes().Select(index => index.GetDatabaseName()).Should().Contain("ix_asp_net_users_invite_link_id");
        roleEntity.GetIndexes().Select(index => index.GetDatabaseName()).Should().Contain("role_name_index");
    }

    [Fact]
    public void BuildModel_ConfiguresApplicationUserDefaults()
    {
        using var context = CreateContext();

        var userEntity = context.Model.FindEntityType(typeof(ApplicationUser));
        var genderDefault = userEntity!.FindProperty(nameof(ApplicationUser.Gender))!.GetDefaultValue();
        var onboardingDefault = userEntity.FindProperty(nameof(ApplicationUser.IsOnboardingComplete))!.GetDefaultValue();

        Convert.ToInt32(genderDefault).Should().Be((int)UserGender.Unspecified);
        onboardingDefault.Should().Be(false);
    }

    [Fact]
    public void BuildModel_BoundsIdentityCompositeKeyColumns()
    {
        using var context = CreateContext();

        var loginEntity = context.Model.FindEntityType(typeof(IdentityUserLogin<Guid>));
        var tokenEntity = context.Model.FindEntityType(typeof(IdentityUserToken<Guid>));

        loginEntity!.FindProperty(nameof(IdentityUserLogin<Guid>.LoginProvider))!.GetMaxLength().Should().Be(128);
        loginEntity.FindProperty(nameof(IdentityUserLogin<Guid>.ProviderKey))!.GetMaxLength().Should().Be(128);
        tokenEntity!.FindProperty(nameof(IdentityUserToken<Guid>.LoginProvider))!.GetMaxLength().Should().Be(128);
        tokenEntity.FindProperty(nameof(IdentityUserToken<Guid>.Name))!.GetMaxLength().Should().Be(128);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=blinder_tests;Username=postgres;Password=postgres",
                npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
