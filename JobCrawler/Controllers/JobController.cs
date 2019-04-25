using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobCrawler.Common;
using JobCrawler.Services;

namespace JobCrawler.Controllers
{
    public class JobController : Controller
    {
        private readonly JobService jobService;

        public JobController()
        {
            jobService = new JobService();
        }

        [HttpGet, Route("api/JobList")]
        public async Task<JsonResult> GetJobList(string city, string keyword, string jobSources, int pageIndex = 1)
        {
            if (jobSources.Length <= 0)
                return new JsonResult(new { State = false, Data = "", Msg = "参数错误" });

            var queryString = Request.QueryString.ToString().Remove(0, 1);
            var result = await jobService.GetJobListFromJobSourceAsync(city, keyword, jobSources, pageIndex, queryString);
            if (result != null && result.Any())
                return new JsonResult(new { State = true, Data = result, Msg = string.Empty });

            return new JsonResult(new { State = false, Data = result, Msg = "没有找到数据" });
        }

        [HttpGet, Route("api/JobDetail")]
        public async Task<JsonResult> GetJobDetail(string url, int jobSource)
        {
            if (string.IsNullOrEmpty(url) || jobSource < 0 || jobSource >= Enum.GetNames(typeof(JobSource)).Length)
                return new JsonResult(new { State = false, Data = "", Msg = "参数错误" });

            var result = await jobService.GetJobDetailFromJobSourceAsync(url, (JobSource)jobSource);
            if (result != null)
                return new JsonResult(new { State = true, Data = result, Msg = string.Empty });

            return new JsonResult(new { State = false, Data = string.Empty, Msg = "没有找到数据" });
        }

    }
}