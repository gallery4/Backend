using PathLib;

public static class Configurations
{
    public static string BaseDirectory { get; } = "./data";
    public static PosixPath BaseDirectoryPath { get; } = new PosixPath(BaseDirectory);

    public static int GridThumbnailWidth { get; } = 400;
    public static int GridThumbnailHeight { get; } = 300;

    public const int ListThumbnailWidth = 96;
    public const int ListThumbnailHeight = 64;

    public static int ViewImageWidth { get; } = 2048;
    public static int ViewImageHeight { get; } = 2048;
}