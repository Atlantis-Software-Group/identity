namespace asg.dbmigrator.Constants;

public static class ErrorMessageConstants
{
    public static string ScriptNameNotProvided = "Script name was not provided. Example usage: -scriptName Name";
    public static string DbContextNameNotProvided = "DbContextName was not provided. Example usage: -dbContextName name";
    public static string MigrationNameNotProvided = "MigrationName was not provideed. Example usage: -migrationName name";
    public static string FileIOError = "Error encountered while attempting to create the script file.";
    public static string FileAlreadyExists = "File already exists";
    public static string DuplicateClass = "Class with the same name already exists.";
}
