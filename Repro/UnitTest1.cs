using Microsoft.Build.Utilities.ProjectCreation;
using System.IO.Abstractions;
using Xunit.Abstractions;

namespace Repro;

public class UnitTest1 : MSBuildTestBase
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test1()
    {
        const string projectName = "ClassLibraryA";
        FileSystem fs = new();

        using (fs.CreateDisposableDirectory(out IDirectoryInfo temp))
        {
            Uri[] feeds = [ new Uri("https://api.nuget.org/v3/index.json") ];

            using (PackageRepository.Create(temp.FullName, feeds))
            {
                ProjectCreator projectCreator = ProjectCreator.Templates.SdkCsproj()
                    // Add this commented out code to prevent automatic generation of Assembly info and see the test pass.
                    //.Property("GenerateAssemblyInfo", "false")
                    .Save(Path.Combine(temp.FullName, projectName, $"{projectName}.csproj"));

                _output.WriteLine("Building for the first time:");
                projectCreator.TryBuild(restore: true, out bool firstResult, out BuildOutput firstOutput);
                Assert.True(firstResult, "This build should always succeed.");
                PrintAssemblyInfo(projectCreator);

                _output.WriteLine("Building for the second time:");
                projectCreator.TryBuild(restore: false, out bool secondResult, out BuildOutput secondOutput);
                PrintAssemblyInfo(projectCreator);
                Assert.True(secondResult, "This rebuild _should_ also succeed, but fails with duplicate version info.");
            }
        }
    }

    private void PrintAssemblyInfo(ProjectCreator projectCreator)
    {
        string objRelativePath = projectCreator.Project.GetPropertyValue("IntermediateOutputPath");
        string objFullPath = Path.GetFullPath(Path.Combine(projectCreator.Project.FullPath, "..", objRelativePath));

        string[] assemblyInfoFiles = Directory.GetFiles(objFullPath, "*AssemblyInfo.cs");

        if (assemblyInfoFiles.Length != 1)
        {
            throw new Exception($"Unable to determine AssemblyInfo.cs file. Found: {string.Join(", ", assemblyInfoFiles)}");
        }

        string contents = File.ReadAllText(assemblyInfoFiles.Single());

        _output.WriteLine("Contents of generated AssemblyInfo.cs:");
        _output.WriteLine(contents);
        _output.WriteLine("----------");
    }
}