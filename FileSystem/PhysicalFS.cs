using Backend.Services;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Net.Http.Headers;
using PathLib;
using System.Net.Mime;

namespace Backend.FileSystem;

public static class PhysicalFS
{
    public static ListResponse List(PosixPath path)
    {
        var directories = new LinkedList<ListResponseItem>();
        var files = new LinkedList<ListResponseItem>();
        var archives = new LinkedList<ListResponseItem>();

        var actualPath = Configurations.BaseDirectoryPath.Join(path);

        foreach (var p in actualPath.ListDir(SearchOption.TopDirectoryOnly))
        {
            if (p.IsDir())
            {
                directories.AddLast(new ListResponseItem
                {
                    Name = p.RelativeTo(Configurations.BaseDirectoryPath).ToString(),
                    DateTime = Timestamp.FromDateTime(p.DirectoryInfo.LastWriteTime.ToUniversalTime())
                });
            }
            else
            {
                if (PathUtility.HasArchiveFileExt(p))
                {
                    archives.AddLast(new ListResponseItem
                    {
                        Name = p.RelativeTo(Configurations.BaseDirectoryPath).ToString(),
                        DateTime = Timestamp.FromDateTime(p.FileInfo.LastWriteTime.ToUniversalTime())
                    });
                }
                else if (PathUtility.IsViewableFile(p))
                {
                    files.AddLast(new ListResponseItem
                    {
                        Name = p.RelativeTo(Configurations.BaseDirectoryPath).ToString(),
                        DateTime = Timestamp.FromDateTime(p.FileInfo.LastWriteTime.ToUniversalTime())
                    });
                }
            }
        }

        var pathString = path.ToString();
        if (pathString == ".")
        {
            pathString = "";
        }

        var output = new ListResponse
        {
            Path = pathString
        };

        output.Directories.AddRange(directories);
        output.Archives.AddRange(archives);
        output.Files.AddRange(files);

        return output;
    }

    public static Stream ReadFile(PosixPath path)
    {
        return Configurations.BaseDirectoryPath.Join(path).FileInfo.OpenRead();
    }
}

