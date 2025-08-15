using Grpc.Core;
using Backend;
using Backend.FileSystem;
using PathLib;

namespace Backend.Services;

public class BrowseService(ILogger<GreeterService> logger) : Browse.BrowseBase
{
    private readonly ILogger<GreeterService> _logger = logger;

    public override async Task<ListResponse> List(ListRequest request, ServerCallContext context)
    {
        var (physicalPath, archivePath, hasArchivePath) =
                   PathUtility.SplitPathAfterArchiveFile(new PosixPath(request.Path));

        if (hasArchivePath)
        {
            return ArchiveFS.List(physicalPath, archivePath);
        }
        else
        {
            return PhysicalFS.List(physicalPath);
        }
    }
}
