using System.Net;

namespace GERMAG.Server.DataPulling;

public interface IFindLocalDirectoryPath
{
    string getLocalPath(string foldername, string filename);
}

public class FindLocalDirectoryPath : IFindLocalDirectoryPath
{
    public string getLocalPath(string folderName, string fileName)
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string targetDirectory = "GERMAG";

        while (!Directory.Exists(Path.Combine(currentDirectory, targetDirectory)))
        {
            DirectoryInfo? parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory == null)
            {
                throw new Exception("Target directory not found.");
            }
            currentDirectory = parentDirectory.FullName;
        }

        string resourcesFile = Path.Combine(currentDirectory, targetDirectory, folderName, fileName);

        return resourcesFile;

    }
}
