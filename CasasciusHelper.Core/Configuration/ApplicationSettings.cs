namespace CasasciusHelper.Core.Configuration;

public class ApplicationSettings
{
    /// <summary>
    /// Location of application's data folder
    /// </summary>
    /// <remarks>This folder is used to store temporary files. The application's database is also supposed to be created there</remarks>
    public string DataFolder { get; set; } = "Data";
}
