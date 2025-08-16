using Backend;
using Backend.FileSystem;
using Google.Protobuf;
using Grpc.Core;
using PathLib;

namespace Backend.Services;

public class ImageService
(ILogger<ImageService> logger) : Image.ImageBase
{
    private readonly ILogger<ImageService> _logger = logger;

    const string MIME_TYPE = "images/webp";
    const int STREAM_SIZE = 10 * 1024;

    public override async Task Thumbnail(
        ThumbnailRequest request, IServerStreamWriter<ImageStreamResponse> responseStream, ServerCallContext context)
    {
        var data = request.ListType switch {
            ListType.List => CreateListThumbnail(
                request.Path, width: request.Width == 0 ? Configurations.ListThumbnailWidth : request.Width,
                height: request.Height == 0 ? Configurations.ListThumbnailHeight : request.Height),

            _ => CreateGridThumbnail(request.Path,
                                     width: request.Width == 0 ? Configurations.GridThumbnailWidth : request.Width,
                                     height: request.Height == 0 ? Configurations.GridThumbnailHeight : request.Height),
        };
        var filename = new PosixPath(request.Path).Filename;

        await WriteStream(responseStream, data, $"{filename}.thumb.webp", MIME_TYPE);

        return;
    }

    public override async Task View(ViewRequest request, IServerStreamWriter<ImageStreamResponse> responseStream,
                                    ServerCallContext context)
    {
        var pathObj = new PosixPath(request.Path);
        var filename = pathObj.Filename;

        if (MimeTypes.GetMimeType(filename) == "image/gif")
        {
            using var stream = Utility.ReadFile(pathObj);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var data = ms.ToArray();

            await WriteStream(responseStream, data, filename, "image/gif");
            return;
        }

        else
        {
            using var stream = Utility.ReadFile(pathObj);
            using var image = NetVips.Image.NewFromStream(stream);

            byte[] output;
            if (image.Width < Configurations.ViewImageWidth || image.Height < Configurations.ViewImageHeight)
            {
                output = image.WebpsaveBuffer();
            }
            else
            {
                using var thumb =
                    image.ThumbnailImage(width: Configurations.ViewImageWidth, height: Configurations.ViewImageHeight,
                                         crop: NetVips.Enums.Interesting.None);

                output = thumb.WebpsaveBuffer();
            }

            await WriteStream(responseStream, output, $"{filename}.view.webp", MIME_TYPE);

            return;
        }
    }

    public byte[] CreateListThumbnail(string path, int width, int height)
    {
        using var stream = Utility.ReadFile(new PosixPath(path));
        using var image = NetVips.Image.NewFromStream(stream);

        var scale =
            image.Width > image.Height ? (double)width / (double)image.Width : (double)height / (double)image.Height;

        using var thumb = image.Resize(scale);

        return thumb.WebpsaveBuffer();
    }

    public byte[] CreateGridThumbnail(string path, int width, int height)
    {
        using var stream = Utility.ReadFile(new PosixPath(path));
        using var image = NetVips.Image.NewFromStream(stream);

        using var thumb = image.ThumbnailImage(width, height, crop: NetVips.Enums.Interesting.Entropy);

        return thumb.WebpsaveBuffer();
    }

    private static async Task WriteStream(IServerStreamWriter<ImageStreamResponse> responseStream, byte[] data,
                                          string filename, string mimeType)
    {
        for (int i = 0; i < data.Length; i += STREAM_SIZE)
        {
            var count = Math.Min(STREAM_SIZE, data.Length - i);

            await responseStream.WriteAsync(new ImageStreamResponse { Filename = filename, ContentType = mimeType,
                                                                      Size = count,
                                                                      Data = ByteString.CopyFrom(data, i, count) });
        }
    }
}
