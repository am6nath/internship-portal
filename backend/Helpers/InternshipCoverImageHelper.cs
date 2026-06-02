using InternshipPortal.API.Entities;

namespace InternshipPortal.API.Helpers
{
    public static class InternshipCoverImageHelper
    {
        private static readonly Dictionary<string, string> ThemeImages = new(StringComparer.OrdinalIgnoreCase)
        {
            ["software"] = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1200",
            ["developer"] = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1200",
            ["programming"] = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=1200",
            ["python"] = "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?w=1200",
            ["java"] = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=1200",
            ["angular"] = "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=1200",
            ["react"] = "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=1200",
            ["data"] = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=1200",
            ["ai"] = "https://images.unsplash.com/photo-1677442136019-21780ecad995?w=1200",
            ["machine learning"] = "https://images.unsplash.com/photo-1677442136019-21780ecad995?w=1200",
            ["cloud"] = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200",
            ["devops"] = "https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=1200",
            ["mobile"] = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=1200",
            ["android"] = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=1200",
            ["ios"] = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=1200",
            ["cyber"] = "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=1200",
            ["security"] = "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=1200",
            ["business"] = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=1200",
            ["marketing"] = "https://images.unsplash.com/photo-1533750349088-cd871a30f63d?w=1200",
            ["finance"] = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?w=1200",
            ["mechanical"] = "https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?w=1200",
            ["electronics"] = "https://images.unsplash.com/photo-1518770660439-4636190af475?w=1200",
            ["civil"] = "https://images.unsplash.com/photo-1541888946425-d81bb19240f5?w=1200",
            ["design"] = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=1200",
            ["ui"] = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=1200",
            ["ux"] = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=1200"
        };

        private const string DefaultImage =
            "https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=1200";

        public static string ResolveCoverImageUrl(Internship internship)
        {
            if (!string.IsNullOrWhiteSpace(internship.CoverImageUrl))
            {
                return internship.CoverImageUrl;
            }

            return GetDynamicCoverImageUrl(internship);
        }

        public static string GetDynamicCoverImageUrl(Internship internship)
        {
            var text = $"{internship.Title} {internship.RequiredSkills} {internship.AllowedDepartments} {internship.CompanyName}"
                .ToLowerInvariant();

            foreach (var (keyword, url) in ThemeImages)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }
            }

            // Deterministic fallback from internship id so the same internship always gets the same image
            var themes = ThemeImages.Values.Distinct().ToList();
            var index = Math.Abs(internship.Id.GetHashCode()) % themes.Count;
            return themes[index];
        }
    }
}
