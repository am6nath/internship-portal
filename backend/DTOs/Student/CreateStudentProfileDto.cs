namespace InternshipPortal.API.DTOs.Student
{
    public class CreateStudentProfileDto
    {
        public string CollegeName { get; set; }

        public string Department { get; set; }

        public decimal CGPA { get; set; }

        public int Backlogs { get; set; }

        public int GraduationYear { get; set; }

        public string Skills { get; set; }
    }
}