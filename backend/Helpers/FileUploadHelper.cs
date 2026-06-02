namespace InternshipPortal.API.Helpers
{
    public static class FileUploadHelper
    {
        public static IFormFile? ResolveFormFile(
            HttpRequest request,
            params string[] preferredFieldNames)
        {
            foreach (var name in preferredFieldNames)
            {
                var file = request.Form.Files.GetFile(name);
                if (file != null && file.Length > 0)
                {
                    return file;
                }
            }

            return request.Form.Files.FirstOrDefault(f => f.Length > 0);
        }
    }
}
