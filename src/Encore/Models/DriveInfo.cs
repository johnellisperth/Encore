
namespace Encore.Models;

public class DriveInfo
{
    public string Drive { get; init; } = string.Empty;
    public string VolumeName { get; init; } = string.Empty;

    public override string ToString()
    {
        return !String.IsNullOrEmpty(Drive) ? $"{VolumeName} ({Drive})" : "Select a drive...";
    }
}

