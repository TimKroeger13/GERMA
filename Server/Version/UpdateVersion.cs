using System.Diagnostics;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;

public interface IUpdateVersion
{
    void UpdateVersionFile();
}

public class UpdateVersion : IUpdateVersion
{
    public void UpdateVersionFile()
    {
        try
        {
            // Check if Git is available
            if (!IsGitInstalled())
            {
                Console.WriteLine("Git is not installed or not available in the PATH. Skipping version file update.");
                return;
            }

            // Run the Git command to get the latest commit info
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "log -1 --format=\"%cd - %s\" --date=iso",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            using var process = Process.Start(processInfo);
            using var reader = process?.StandardOutput;
            var result = reader?.ReadToEnd().Trim();

            process?.WaitForExit();

            if (!string.IsNullOrEmpty(result))
            {
                // Write the result to the .version file 
                File.WriteAllText(".version", result);
                Console.WriteLine("Version file updated successfully.");
            }
            else
            {
                Console.WriteLine("No Git version information available.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update .version file: {ex.Message}");
        }
    }

    private bool IsGitInstalled()
    {
        try
        {
            // Check if `git --version` runs successfully
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            process?.WaitForExit();

            return process?.ExitCode == 0; // Exit code 0 indicates success
        }
        catch
        {
            return false; // If an exception occurs, Git is not available
        }
    }


}