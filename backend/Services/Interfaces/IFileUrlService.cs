namespace InternshipPortal.API.Services.Interfaces
{
    public interface IFileUrlService
    {
        string ToPublicUrl(string? relativeOrAbsolutePath);

        string? ToStoredPath(string? publicUrl);
    }
}
