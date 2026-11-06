using System.Reflection;

namespace HalloweenPasswords.Tests;

public class ProjectSetupTests
{
    [Fact]
    public void Api_project_is_available_to_test_project()
    {
        var apiAssembly = Assembly.Load("HalloweenPasswords.Api");

        Assert.Equal("HalloweenPasswords.Api", apiAssembly.GetName().Name);
    }
}
