using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests
{
    [TestFixture]
    public class PackageCheck
    {
        [Test]
        //[Ignore("Ignored so that this test will not run on the build server, uncomment to use this check locally.")]
        public void CheckForVulnerableAndDeprecatedPackages()
        {
            string projectDir = @"..\..\..\..\";

            var csprojs = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);

            foreach (var csprojPath in csprojs)
            {
                Console.WriteLine($"Checking: {csprojPath}");

                var result = RunCheck(csprojPath);

                result.Should().NotBeNullOrWhiteSpace();
            }
        }

        private string RunCheck(string projectFilePath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"list \"{projectFilePath}\" package --source nuget.org --vulnerable",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed for {projectFilePath}:\n{error}");
            }

            return output;
        }
    }
}
