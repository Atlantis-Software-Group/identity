using asg.dbmigrator.Constants;
using asg.dbmigrator.CreateSeedTemplate.Interfaces;
using Microsoft.Extensions.Logging;

namespace asg.dbmigrator.CreateSeedTemplate.Services;

public class FileProviderService : IFileProviderService
{
    public FileProviderService(ILogger<FileProviderService> logger)
    {
        Logger = logger;
    }

    public ILogger<FileProviderService> Logger { get; }
    public Exception? Error { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<bool> CreateFile(string fullPath, byte[] content)
    {
        if ( FileExits(fullPath) )
        {
            ErrorMessage = ErrorMessageConstants.FileAlreadyExists;
            return false; 
        }

        try
        {
            await File.WriteAllBytesAsync(fullPath, content);
        }
        catch (Exception e)
        {
            Error = e;
            ErrorMessage = e.Message;
            Logger                
                .LogError(e, "Error encountered while attempting to write seedscript to disk. Filename: {file}", fullPath);
            
            return false;
        }

        return true;
    }

    public bool DirectoryExists(string fullPath)
    {
        foreach ( string path in Directory.GetDirectories(fullPath) )
        {
            if ( !Directory.Exists(path) )
                return false;
        }
        return true;
    }

    public bool FileExits(string fullPath)
    {
        return File.Exists(fullPath);
    }
}
