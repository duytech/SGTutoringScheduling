using System.Reflection;
using TutoringScheduling.Application;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Tests;

/// <summary>
/// Turns the Clean Architecture dependency rule into a test: the inner layers
/// must not drag in the persistence framework or the web stack.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly Application = typeof(IScheduleService).Assembly;
    private static readonly Assembly Domain = typeof(Booking).Assembly;

    [Fact]
    public void Application_doesNotReference_EntityFrameworkCore()
    {
        Assert.DoesNotContain(
            Application.GetReferencedAssemblies(),
            name => name.Name!.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_doesNotReference_AspNetCore()
    {
        Assert.DoesNotContain(
            Application.GetReferencedAssemblies(),
            name => name.Name!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Domain_onlyReferences_TheBaseClassLibrary()
    {
        Assert.All(
            Domain.GetReferencedAssemblies(),
            name => Assert.True(
                name.Name is "System.Private.CoreLib" or "System.Runtime" or "netstandard",
                $"Domain unexpectedly references {name.Name}."));
    }
}
