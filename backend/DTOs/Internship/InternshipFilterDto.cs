namespace InternshipPortal.API.DTOs.Internship
{
    public class InternshipFilterDto
    {
        public string? Search { get; set; }

        public string? Department { get; set; }

        public decimal? MinimumCGPA { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public string? CompanyName { get; set; }

        public string? Location { get; set; }

        public string? SortBy { get; set; }

        public bool Descending { get; set; }

        /// <summary>When true, only internships open for registration are returned.</summary>
        public bool OpenOnly { get; set; } = false;

    }
}