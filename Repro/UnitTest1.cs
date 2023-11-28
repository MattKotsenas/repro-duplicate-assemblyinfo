using Microsoft.Build.Utilities.ProjectCreation;
using System.IO.Abstractions;

namespace Repro;

public class UnitTest1 : MSBuildTestBase
{
    [Fact]
    public void Test1()
    {
        FileSystem fs = new();
        using (fs.CreateDisposableDirectory(out IDirectoryInfo temp))
        {
            Uri[] feeds = [ new Uri("https://api.nuget.org/v3/index.json") ];

            using (PackageRepository.Create(temp.FullName, feeds))
            {
                var projectCreator = ProjectCreator.Templates.SdkCsproj()
                    // Add this commented out code to prevent automatic generation of Assembly info and see the test pass.
                    //.Property("GenerateAssemblyInfo", "false")
                    .Save(Path.Combine(temp.FullName, "ClassLibraryA", "ClassLibraryA.csproj"));

                projectCreator.TryBuild(restore: true, out bool initialBuildResult, out BuildOutput output);
                Assert.True(initialBuildResult);

                projectCreator.TryBuild(restore: false, out bool secondBuildResult, out BuildOutput output2);
                Console.WriteLine(output2.GetConsoleLog());
                Assert.True(secondBuildResult);
            }
        }
    }
}