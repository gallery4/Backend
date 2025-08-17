using PathLib;

public static class Configurations
{
    public static string BaseDirectory
    {
        get => DotNetEnv.Env.GetString("DATA_DIR", "./data");
    }
    public static PosixPath BaseDirectoryPath { get => new PosixPath(BaseDirectory).Resolve(); }

    public static int GridThumbnailWidth { get; } = 400;
    public static int GridThumbnailHeight { get; } = 300;

    public const int ListThumbnailWidth = 96;
    public const int ListThumbnailHeight = 64;

    public static int ViewImageWidth { get; } = 2048;
    public static int ViewImageHeight { get; } = 2048;
}