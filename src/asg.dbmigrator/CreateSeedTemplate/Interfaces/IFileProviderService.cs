namespace asg.dbmigrator.CreateSeedTemplate.Interfaces;

public interface IFileProviderService
{
    Exception? Error { get; set; }
    string? ErrorMessage { get; set; }
    /// <summary>
    /// Determine if the file already exists
    /// </summary>
    /// <param name="fullPath">Full path to a file</param>
    /// <returns>True if the file exists, otherwise false. </returns>
    bool FileExits(string fullPath);

    /// <summary>
    /// Determines if all the directories along the path exist. 
    /// </summary>
    /// <param name="fullPath">Full path to a file</param>
    /// <returns>True if all the directories exist, otherwise false. </returns>
    bool DirectoryExists(string fullPath);

    /// <summary>
    /// Write a file to disk
    /// </summary>
    /// <param name="fullPath">Full path of a file to be created</param>
    /// <param name="content">Content of the file</param>
    /// <returns>True if file is created successfully. Otherwise false</returns>
    Task<bool> CreateFile(string fullPath, byte[] content);
}
