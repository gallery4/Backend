using Microsoft.Net.Http.Headers;
using PathLib;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System.IO;
using Backend.Services;
using Google.Protobuf.WellKnownTypes;

namespace Backend.FileSystem
{
    public class ArchiveFS
    {
        public static ListResponse List(PosixPath physicalPath, PosixPath archivePath)
        {
            var actualPath = Configurations.BaseDirectoryPath.Join(physicalPath);
            using var archive = OpenArchive(actualPath);

            var directorySet = new HashSet<ListResponseItem>();
            var files = new LinkedList<ListResponseItem>();

            var archivePathStr = archivePath.ToString();
            if (archivePathStr == ".")
            {
                archivePathStr = "";
            }

            var response = new ListResponse
            {
                Path = physicalPath.Join(archivePath).ToString(),
            };

            foreach (var e in archive.Entries)
            {
                var entryPath = new PosixPath(e.Key);

                if (entryPath.Directory == archivePathStr)
                {
                    if (e.IsDirectory)
                    {
                        directorySet.Add(new ListResponseItem
                        {
                            Name = physicalPath.Join(entryPath).ToString(),
                            DateTime = Timestamp.FromDateTime(e.LastModifiedTime ?? DateTime.Now),
                        });
                    }
                    else if (PathUtility.IsViewableFile(entryPath))
                    {
                        files.AddLast(new ListResponseItem
                        {
                            Name = physicalPath.Join(entryPath).ToString(),
                            DateTime = Timestamp.FromDateTime(e.LastModifiedTime ?? DateTime.Now)
                        });
                    }
                }
            }

            response.Directories.AddRange(directorySet);
            response.Files.AddRange(files);

            return response;
        }

        public static Stream ReadFile(PosixPath physicalPath, PosixPath archivePath)
        {
            using IArchive archive = OpenArchive(Configurations.BaseDirectoryPath.Join(physicalPath));

            var entry =
                archive.Entries.First((e) => e.Key == archivePath.ToString()) ?? throw new Exception("entry not found");

            var stream = entry.OpenEntryStream();
            var outstream = new MemoryStream();

            stream.CopyTo(outstream);
            outstream.Position = 0;

            return outstream;
        }

        private static IArchive OpenArchive(PosixPath path)
        {
            return path.Extension switch
            {
                ".cbz" => ZipArchive.Open(path.ToString()),
                ".cbr" => RarArchive.Open(path.ToString()),
                _ => ArchiveFactory.Open(path.ToString())
            };
        }

        public static IResult SendFile(HttpContext http, PosixPath archivePath, PosixPath entryPath)
        {
            var disposition = new ContentDispositionHeaderValue(dispositionType: "inline");
            disposition.SetHttpFileName(entryPath.Filename);

            http.Response.Headers.ContentDisposition = disposition.ToString();

            var steam = ReadFile(archivePath, entryPath);

            return Results.Stream(steam, enableRangeProcessing: true,
                                  contentType: MimeTypes.GetMimeType(entryPath.Filename));
        }
    }
}
