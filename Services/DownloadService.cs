using Grpc.Core;
using Backend;
using Backend.FileSystem;
using PathLib;

namespace Backend.Services;

public class DownloadService(ILogger<GreeterService> logger) : Download.DownloadBase
{
    private readonly ILogger<GreeterService> _logger = logger;
    const int STREAM_SIZE = 10 * 1024;

    public override async Task Get(GetRequest request, IServerStreamWriter<DownloadStreamResponse> responseStream, ServerCallContext context)
    {
        var pathObj = new PosixPath(request.Path);
        var filename = pathObj.Filename;

        using var stream = GetStream(pathObj);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var data = ms.ToArray();

        await WriteStream(responseStream, data, filename);
        return;
    }

    private static async Task WriteStream(IServerStreamWriter<DownloadStreamResponse> responseStream, byte[] data, string filename)
    {
        for (int i = 0; i < data.Length; i += STREAM_SIZE)
        {
            var count = Math.Max(STREAM_SIZE, data.Length - i);

            await responseStream.WriteAsync(new ImageStreamResponse
            {
                Filename = filename,
                Size = count,
                Data = ByteString.CopyFrom(data, i, count)
            });
        }
    }
}
