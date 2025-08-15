using Backend;
using PathLib;

public static class PathUtility
{
    public static (PosixPath physicalPath, PosixPath archivePath, bool hasArchivePath)
        SplitPathAfterArchiveFile(this PosixPath path)
    {
        if (HasArchiveFileExt(path))
        {
            return (physicalPath: path, archivePath: new PosixPath(), hasArchivePath: true);
        }

        foreach (var p in path.Parents())
        {
            if (HasArchiveFileExt(p))
            {
                var physicalPath = p;
                var archivePath = path.RelativeTo(physicalPath);

                return (physicalPath, archivePath, true);
            }
        }

        return (path, new PosixPath(), false);
    }

    public static bool HasArchiveFileExt(string path)
    {
        return path.EndsWith(".zip") || path.EndsWith(".cbz") || path.EndsWith(".rar") || path.EndsWith(".cbr") ||
               path.EndsWith(".7z");
    }

    public static bool HasArchiveFileExt(PosixPath path)
    {
        return path.Extension == ".zip" || path.Extension == ".cbz" || path.Extension == ".rar" ||
               path.Extension == ".cbr" || path.Extension == ".7z";
    }

    public static bool IsViewableFile(PosixPath path)
    {
        var mimetype = MimeTypes.GetMimeType(path.Filename);

        if (mimetype.StartsWith("image/") || mimetype.StartsWith("video/") || mimetype.StartsWith("audio/") ||
            mimetype == "application/pdf" || mimetype == "application/epub+zip")
        {
            return true;
        }
        return false;
    }
}
