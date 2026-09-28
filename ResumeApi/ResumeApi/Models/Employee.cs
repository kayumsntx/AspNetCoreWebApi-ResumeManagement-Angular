using System.ComponentModel.DataAnnotations;

namespace ResumeApi.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public bool IsActive { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JoinDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageName { get; set; }
        public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    }
    public class ExperienceTitle
    {
        public int ExperienceTitleId { get; set; }
        public string TitleName { get; set; } = null!;
        public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    }
    public class Experience
    {
        public int ExperienceId { get; set; }
        public int EmployeeId { get; set; }
        public int ExperienceTitleId { get; set; }
        public int Duration { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public virtual ExperienceTitle ExperienceTitle { get; set; } = null!;
    }
}
