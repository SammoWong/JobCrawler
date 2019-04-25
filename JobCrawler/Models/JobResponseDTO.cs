using JobCrawler.Common;

namespace JobCrawler.Models
{
    public class JobResponseDTO
    {
        public string JobTitle { get; set; }

        public string CompanyName { get; set; }

        public string Salary { get; set; }

        public string WorkingPlace { get; set; }

        public string ReleaseDate { get; set; }

        public string DetailUrl { get; set; }

        public JobSource JobSource { get; set; }

        public string JobSourceText { get; set; }
    }
}
