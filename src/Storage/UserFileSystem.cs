
namespace Storage
{
    public static class UserFileSystem
    {
        public static string[] PCDriveList = DriveInfo.GetDrives().Select(dd => dd.Name).ToArray();
    }  
}
