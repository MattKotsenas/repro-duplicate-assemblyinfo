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
            Uri[] feeds = new[]
            {
                    new Uri("https://api.nuget.org/v3/index.json")
            };

            using (PackageRepository.Create(temp.FullName, feeds))
            {
                var projectCreator = ProjectCreator.Templates.SdkCsproj()
                    //.Property("GenerateAssemblyInfo", "false") // Double-building results in duplicate defintions of assembly info. Since our tests don't rely on assembly info, skip generating it
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