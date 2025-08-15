namespace Backend.FileSystem;

using PathLib;

static class Utility
{
    public static Stream ReadFile(PosixPath path)
    {
        var (physicalPath, archivePath, hasArchivePath) = PathUtility.SplitPathAfterArchiveFile(path);

        if (hasArchivePath)
        {
            return ArchiveFS.ReadFile(physicalPath, archivePath);
        }
        else
        {
            return PhysicalFS.ReadFile(physicalPath);
        }
    }
}