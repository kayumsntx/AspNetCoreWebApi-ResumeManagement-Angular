using System.ComponentModel.DataAnnotations;

namespace ResumeApi.Models.DTOs
{
    public class EmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public bool IsActive { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JoinDate { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string EXperiencesString { get; set; } = "[]";
    }
    public class ExperienceDTO
    {
        public int ExperienceTitleId { get; set; }
        public int Duration { get; set; }
    }
}
