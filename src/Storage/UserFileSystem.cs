
namespace Storage;

public static class UserFileSystem
{
    public static string[] PCDriveList = DriveInfo.GetDrives().Select(dd => dd.Name).ToArray();

    public static (string, string)[] PCDetailedDriveList = DriveInfo.GetDrives().Select(dd => (dd.Name, string.IsNullOrEmpty(dd.VolumeLabel) ? "Local Disk": dd.VolumeLabel)).ToArray();

}

