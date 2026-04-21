using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Blinder.ArchitectureTests;

/// <summary>
/// Architecture boundary tests ensuring no cross-boundary drift between Blinder projects.
/// These tests run early to catch structural violations before they compound.
/// </summary>
public class BoundaryTests
{
    // Assembly anchors via project references (assemblies are copied to test output directory)
    private static readonly Assembly SharedKernelAssembly = typeof(Blinder.SharedKernel.Error).Assembly;
    private static readonly Assembly ContractsAssembly = Assembly.Load(new AssemblyName("Blinder.Contracts"));
    private static readonly Assembly IdentityServerAssembly = Assembly.Load(new AssemblyName("Blinder.IdentityServer"));
    private static readonly Assembly ApiAssembly = Assembly.Load(new AssemblyName("Blinder.Api"));
    private static readonly Assembly AdminPanelAssembly = Assembly.Load(new AssemblyName("Blinder.AdminPanel"));

    [Fact]
    public void SharedKernel_MustNotDependOnAnyOtherBlinderProject()
    {
        // SharedKernel is the foundational layer — it must have zero dependencies on other Blinder assemblies.
        var forbiddenDependencies = new[]
        {
            "Blinder.Contracts",
            "Blinder.IdentityServer",
            "Blinder.Api",
            "Blinder.AdminPanel"
        };

        foreach (var forbidden in forbiddenDependencies)
        {
            var result = Types.InAssembly(SharedKernelAssembly)
                .Should()
                .NotHaveDependencyOn(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Blinder.SharedKernel must not reference '{forbidden}'. " +
                $"SharedKernel is the foundation layer — it depends on nothing else in the Blinder codebase. " +
                $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Contracts_MustNotDependOnAnyAppProject()
    {
        // Contracts is for shared cross-process contracts only.
        // It must not depend on app-specific assemblies; app-internal types must not live here.
        var appAssemblies = new[]
        {
            "Blinder.IdentityServer",
            "Blinder.Api",
            "Blinder.AdminPanel"
        };

        foreach (var appAssembly in appAssemblies)
        {
            var result = Types.InAssembly(ContractsAssembly)
                .Should()
                .NotHaveDependencyOn(appAssembly)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Blinder.Contracts must not depend on '{appAssembly}'. " +
                $"Contracts holds only shared cross-process types. App-internal types belong in their own project. " +
                $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Contracts_MustRemainEmpty_ForStory11()
    {
        var contractTypes = ContractsAssembly
            .GetTypes()
            .Where(static type => !type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
            .ToArray();

        Assert.Empty(contractTypes);
    }

    [Fact]
    public void Api_MustNotDirectlyDependOnIdentityServerOrAdminPanel()
    {
        AssertAssemblyDoesNotDependOn(
            ApiAssembly,
            "Blinder.IdentityServer",
            "Blinder.AdminPanel");
    }

    [Fact]
    public void Api_MustNotDependOnIdentityServerMigrationNamespaces()
    {
        AssertAssemblyDoesNotDependOn(
            ApiAssembly,
            "Blinder.IdentityServer.Persistence.Migrations",
            "Blinder.IdentityServer.Persistence.DesignTime");
    }

    [Fact]
    public void IdentityServer_MustNotDirectlyDependOnApiOrAdminPanel()
    {
        AssertAssemblyDoesNotDependOn(
            IdentityServerAssembly,
            "Blinder.Api",
            "Blinder.AdminPanel");
    }

    [Fact]
    public void IdentityServer_MustNotDependOnApiMigrationNamespaces()
    {
        AssertAssemblyDoesNotDependOn(
            IdentityServerAssembly,
            "Blinder.Api.Persistence.Migrations",
            "Blinder.Api.Persistence.DesignTime");
    }

    [Fact]
    public void AdminPanel_MustNotDirectlyDependOnIdentityServerOrApi()
    {
        AssertAssemblyDoesNotDependOn(
            AdminPanelAssembly,
            "Blinder.IdentityServer",
            "Blinder.Api");
    }

    /// <summary>
    /// Verifies that an assembly does not take on forbidden Blinder project dependencies.
    /// </summary>
    private static void AssertAssemblyDoesNotDependOn(Assembly assembly, params string[] forbiddenDependencies)
    {
        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(forbiddenDependency)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{assembly.GetName().Name} must not reference '{forbiddenDependency}'. " +
                $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }
}
