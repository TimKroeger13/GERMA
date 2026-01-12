using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.DataModel;
using GERMAG.Server.DataPulling;
using GERMAG.Server.Research;
using Microsoft.AspNetCore.Cors;
using System.Diagnostics;
using System.IO;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/")]
public class TestController() : Controller
{
    [HttpGet("version")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public string VersionTracking()
    {
        var lastCommitDate = GetLastCommitDate();
        return $"Server is running...\nExecuting GERMA version from the Patch: {lastCommitDate}";
    }

    private string GetLastCommitDate()
    {
        try
        {
            // Try to get version from git command first
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "log -1 --format=\"%cd - %s\" --date=iso",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                using var process = Process.Start(processInfo);
                using var outputReader = process?.StandardOutput;
                var result = outputReader?.ReadToEnd().Trim().Replace("_", " ");
                process?.WaitForExit();

                if (!string.IsNullOrEmpty(result))
                    return result;
            }
            catch
            {
                // Ignore and fall back to .version file
            }

            // Search upwards for version.txt
            string currentDir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(currentDir))
            {
                string filePath = Path.Combine(currentDir, "version.txt");
                if (System.IO.File.Exists(filePath))
                    return System.IO.File.ReadAllText(filePath).Trim();

                string? parent = Directory.GetParent(currentDir)?.FullName;
                // Break if at root
                if (string.IsNullOrEmpty(parent) || parent == currentDir)
                    break;

                currentDir = parent;
            }

            return "Version not Found";
    
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

}
