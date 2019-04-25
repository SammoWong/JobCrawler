using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using JobCrawler.Common;
using JobCrawler.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JobCrawler.Services
{
    public class JobService
    {
        public async Task<List<JobResponseDTO>> GetJobListFromJobSourceAsync(string city, string keyword, string jobSources, int pageIndex, string key)
        {
            //if (RedisHelper.Exists(key))
            //return RedisHelper.Get<List<JobResponseDTO>>(key);

            var sources = jobSources.Split("-").ToList();
            var result = new List<JobResponseDTO>();
            try
            {
                foreach (var item in sources)
                {
                    switch (item)
                    {
                        case "1":
                            result.AddRange(await GetJobListFromZhilianAsync(city, keyword, pageIndex));
                            break;
                        case "2":
                            result.AddRange(await GetJobListFromQianchengAsync(city, keyword, pageIndex));
                            break;
                        case "3":
                            result.AddRange(await GetJobListFromLiepinAsync(city, keyword, pageIndex));
                            break;
                        case "4":
                            result.AddRange(await GetJobListFromBossAsync(city, keyword, pageIndex));
                            break;
                        case "5":
                            result.AddRange(await GetJobListFromLagouAsync(city, keyword, pageIndex));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
            //RedisHelper.Set(key, result, 10);
            return result;
        }

        public async Task<JobDetailResponseDTO> GetJobDetailFromJobSourceAsync(string url, JobSource jobSource)
        {
            if (RedisHelper.Exists(url))
                return RedisHelper.Get<JobDetailResponseDTO>(url);

            JobDetailResponseDTO result = null;
            try
            {
                switch (jobSource)
                {
                    case JobSource.智联:
                        result = await GetJobDetailFromZhilianAsync(url);
                        break;
                    case JobSource.前程:
                        result = await GetJobDetailFromQianchengAsync(url);
                        break;
                    case JobSource.猎聘:
                        result = await GetJobDetailFromLiepinAsync(url);
                        break;
                    case JobSource.BOSS:
                        result = await GetJobDetailFromBossAsync(url);
                        break;
                    case JobSource.拉勾:
                        result = await GetJobDetailFromLagouAsync(url);
                        break;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }

            RedisHelper.Set(url, result, 10);
            return result;
        }

        #region 获取职位列表

        private async Task<List<JobResponseDTO>> GetJobListFromZhilianAsync(string city, string keyword, int pageIndex)
        {
            var cityCode = CityCode.GetCityCode(JobSource.智联, city);
            var url = string.Format("http://www.liepin.com/zhaopin/?key={0}&dqs={1}&curPage={2}", keyword, cityCode, pageIndex);
            using (var http = new HttpClient())
            {
                var htmlString = await http.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobs = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".sojob-result .sojob-list")
                    .Where(a => a.QuerySelectorAll(".job-info h3").FirstOrDefault() != null)
                    .Select(b => new JobResponseDTO()
                    {
                        JobTitle = ReplaceHtmlTag(b.QuerySelectorAll("h3 a").FirstOrDefault().TextContent),
                        CompanyName = b.QuerySelectorAll(".company-info .company-name a").FirstOrDefault().TextContent,
                        Salary = b.QuerySelectorAll(".condition .text-warning").FirstOrDefault().TextContent,
                        WorkingPlace = b.QuerySelectorAll(".condition a").FirstOrDefault().TextContent,
                        ReleaseDate = b.QuerySelectorAll(".time-info time").FirstOrDefault().TextContent,
                        DetailUrl = b.QuerySelectorAll(".company-info .company-name a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                        JobSource = JobSource.智联,
                        JobSourceText = "智联"
                    }).ToList();

                return jobs;
            }
        }

        private async Task<List<JobResponseDTO>> GetJobListFromQianchengAsync(string city, string keyword, int pageIndex)
        {
            var cityCode = CityCode.GetCityCode(JobSource.前程, city);
            string url = string.Format("http://search.51job.com/jobsearch/search_result.php?jobarea={0}&keyword={1}&curr_page={2}", cityCode, keyword, pageIndex);
            using (HttpClient httpClient = new HttpClient())
            {
                var htmlBytes = await httpClient.GetByteArrayAsync(url);
                var htmlString = Encoding.GetEncoding("GBK").GetString(htmlBytes);
                HtmlParser htmlParser = new HtmlParser();
                var jobs = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".dw_table div.el")
                    .Where(a => a.QuerySelectorAll(".t1 span a").FirstOrDefault() != null)
                    .Select(b => new JobResponseDTO()
                    {
                        JobTitle = b.QuerySelectorAll(".t1 span a").FirstOrDefault().TextContent,
                        CompanyName = b.QuerySelectorAll(".t2 a").FirstOrDefault().TextContent,
                        WorkingPlace = b.QuerySelectorAll(".t3").FirstOrDefault().TextContent,
                        Salary = b.QuerySelectorAll(".t4").FirstOrDefault().TextContent,
                        ReleaseDate = b.QuerySelectorAll(".t5").FirstOrDefault().TextContent,
                        DetailUrl = b.QuerySelectorAll(".t1 span a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                        JobSource = JobSource.前程,
                        JobSourceText = "前程无忧"
                    }).ToList();

                return jobs;
            }
        }

        private async Task<List<JobResponseDTO>> GetJobListFromLiepinAsync(string city, string keyword, int pageIndex)
        {
            var cityCode = CityCode.GetCityCode(JobSource.猎聘, city);
            string url = string.Format("http://www.liepin.com/zhaopin/?key={0}&dqs={1}&curPage={2}", keyword, cityCode, pageIndex);
            using (HttpClient httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobs = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll("ul.sojob-list li")
                    .Where(a => a.QuerySelectorAll(".job-info h3 a").FirstOrDefault() != null)
                    .Select(b => new JobResponseDTO()
                    {
                        JobTitle = b.QuerySelectorAll(".job-info h3 a").FirstOrDefault().TextContent,
                        CompanyName = b.QuerySelectorAll(".company-name a").FirstOrDefault().TextContent,
                        Salary = b.QuerySelectorAll(".text-warning").FirstOrDefault().TextContent,
                        WorkingPlace = b.QuerySelectorAll(".area").FirstOrDefault().TextContent,
                        ReleaseDate = b.QuerySelectorAll(".time-info time").FirstOrDefault().TextContent,
                        DetailUrl = b.QuerySelectorAll(".job-info h3 a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                        JobSource = JobSource.猎聘,
                        JobSourceText = "猎聘"
                    }).ToList();

                return jobs;
            }
        }

        private async Task<List<JobResponseDTO>> GetJobListFromBossAsync(string city, string keyword, int pageIndex)
        {
            var cityCode = CityCode.GetCityCode(JobSource.BOSS, city);
            string url = string.Format("http://www.zhipin.com/c{0}/h_{0}/?query={1}&page={2}", cityCode, keyword, pageIndex);
            using (var httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobs = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".job-list ul li")
                    .Where(a => a.QuerySelectorAll(".info-primary h3").FirstOrDefault() != null)
                    .Select(b => new JobResponseDTO()
                    {
                        JobTitle = b.QuerySelectorAll(".info-primary h3").FirstOrDefault().TextContent,
                        CompanyName = b.QuerySelectorAll(".company-text h3").FirstOrDefault().TextContent,
                        Salary = b.QuerySelectorAll(".info-primary h3 .red").FirstOrDefault().TextContent,
                        WorkingPlace = b.QuerySelectorAll(".info-primary p").FirstOrDefault().TextContent,
                        ReleaseDate = b.QuerySelectorAll(".job-time .time").FirstOrDefault().TextContent,
                        DetailUrl = "http://www.zhipin.com" + b.QuerySelectorAll("a").FirstOrDefault().Attributes.FirstOrDefault(f => f.Name == "href").Value,
                        JobSource = JobSource.BOSS,
                        JobSourceText = "BOSS直聘"
                    }).ToList();

                return jobs;
            }
        }

        private async Task<List<JobResponseDTO>> GetJobListFromLagouAsync(string city, string keyword, int pageIndex)
        {
            StringContent formurlcontent = new StringContent("first=false&pn=" + pageIndex + "&kd=" + keyword);
            formurlcontent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var url = string.Format("https://www.lagou.com/jobs/positionAjax.json?px=new&city={0}&needAddtionalResult=false&isSchoolJob=0", city);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Referer", "https://www.lagou.com/jobs/list_.net");
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0");
                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                var responseMsg = await httpClient.PostAsync(url, formurlcontent);
                var htmlString = await responseMsg.Content.ReadAsStringAsync();
                var lagouResponse = JsonConvert.DeserializeObject<LagouResponseDTO>(htmlString);
                var result = lagouResponse.Content.PositionResult.Result;
                var jobs = result.Select(r => new JobResponseDTO()
                {
                    JobTitle = r.PositionName,
                    CompanyName = r.CompanyFullName,
                    Salary = r.Salary,
                    WorkingPlace = r.District + (r.BusinessZones == null ? "" : r.BusinessZones.Count <= 0 ? "" : r.BusinessZones[0]),
                    ReleaseDate = DateTime.Parse(r.CreateTime).ToString("yyyy-MM-dd"),
                    DetailUrl = "https://www.lagou.com/jobs/" + r.PositionId + ".html",
                    JobSource = JobSource.拉勾,
                    JobSourceText = "拉勾"
                }).ToList();

                return jobs;
            }
        }

        #endregion

        #region 获取职位详细信息
        private async Task<JobDetailResponseDTO> GetJobDetailFromZhilianAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobDetail = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".terminalpage")
                    .Where(a => a.QuerySelectorAll(".terminalpage-left .terminal-ul li").FirstOrDefault() != null)
                    .Select(b => new JobDetailResponseDTO()
                    {
                        Education = b.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                        Experience = b.QuerySelectorAll(".terminalpage-left .terminal-ul li")[4].TextContent,
                        CompanyNature = b.QuerySelectorAll(".terminalpage-right .terminal-company li")[1].TextContent,
                        CompanySize = b.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                        Requirement = b.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[0].TextContent.Replace("职位描述：", string.Empty),
                        CompanyIntroduction = b.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent
                    }).FirstOrDefault();

                return jobDetail;
            }
        }

        private async Task<JobDetailResponseDTO> GetJobDetailFromQianchengAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var htmlBytes = await httpClient.GetByteArrayAsync(url);
                //【注意】使用GBK需要 Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册编码提供程序
                var htmlString = Encoding.GetEncoding("GBK").GetString(htmlBytes);
                HtmlParser htmlParser = new HtmlParser();
                var jobDetail = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".tCompanyPage")
                    .Where(a => a.QuerySelectorAll(".tBorderTop_box .t1 span").FirstOrDefault() != null)
                    .Select(b => new JobDetailResponseDTO()
                    {
                        //Experience = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[4].TextContent,
                        Education = b.QuerySelectorAll(".tBorderTop_box .t1 span")[0].TextContent,
                        CompanyNature = b.QuerySelectorAll(".msg.ltype")[0].TextContent,
                        //CompanySize = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                        Requirement = b.QuerySelectorAll(".bmsg.job_msg.inbox")[0].TextContent.Replace("职位描述：", ""),
                        CompanyIntroduction = b.QuerySelectorAll(".tmsg.inbox")[0].TextContent
                    }).FirstOrDefault();

                return jobDetail;
            }
        }

        private async Task<JobDetailResponseDTO> GetJobDetailFromLiepinAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobDetail = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll(".wrap")
                    .Where(a => a.QuerySelectorAll(".job-qualifications").FirstOrDefault() != null)
                    .Select(a => new JobDetailResponseDTO()
                    {
                        Experience = a.QuerySelectorAll(".job-qualifications span")[1].TextContent,
                        Education = a.QuerySelectorAll(".job-qualifications span")[0].TextContent,
                        CompanyNature = a.QuerySelectorAll(".new-compintro li")[0].TextContent,
                        CompanySize = a.QuerySelectorAll(".new-compintro li")[1].TextContent,
                        Requirement = a.QuerySelectorAll(".job-item.main-message").FirstOrDefault().TextContent.Replace("职位描述：", ""),
                        CompanyIntroduction = a.QuerySelectorAll(".job-item.main-message.noborder").FirstOrDefault().TextContent,
                    }).FirstOrDefault();

                return jobDetail;
            }
        }

        private async Task<JobDetailResponseDTO> GetJobDetailFromBossAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobDetail = (await htmlParser.ParseAsync(htmlString)).QuerySelectorAll("#main")
                    .Where(t => t.QuerySelectorAll(".job-banner .info-primary p").FirstOrDefault() != null)
                    .Select(t => new JobDetailResponseDTO()
                    {
                        Experience = t.QuerySelectorAll(".job-banner .info-primary p").FirstOrDefault().TextContent,
                        //Education = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                        CompanyNature = t.QuerySelectorAll(".job-banner .info-company p").FirstOrDefault().TextContent,
                        //CompanySize = t.QuerySelectorAll(".terminalpage-right .terminal-company li")[0].TextContent,
                        Requirement = t.QuerySelectorAll(".detail-content div.text").FirstOrDefault().TextContent.Replace("职位描述：", ""),
                        //CompanyIntroduction = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent,
                    })
                    .FirstOrDefault();

                return jobDetail;
            }
        }

        private async Task<JobDetailResponseDTO> GetJobDetailFromLagouAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0");
                var htmlString = await httpClient.GetStringAsync(url);
                HtmlParser htmlParser = new HtmlParser();
                var jobDetail = (await htmlParser.ParseAsync(htmlString))
                    .QuerySelectorAll("body")
                    .Select(t => new JobDetailResponseDTO()
                    {
                        Experience = t.QuerySelectorAll(".job_request p").FirstOrDefault()?.TextContent,
                        //Education = t.QuerySelectorAll(".terminalpage-left .terminal-ul li")[5].TextContent,
                        CompanyNature = t.QuerySelectorAll(".job_company .c_feature li")?.Length <= 0 ? "" : t.QuerySelectorAll(".job_company .c_feature li")[0]?.TextContent,
                        CompanySize = t.QuerySelectorAll(".job_company .c_feature li")?.Length <= 2 ? "" : t.QuerySelectorAll(".job_company .c_feature li")[2]?.TextContent,
                        Requirement = t.QuerySelectorAll(".job_bt div").FirstOrDefault()?.TextContent.Replace("职位描述：", ""),
                        //CompanyIntroduction = t.QuerySelectorAll(".tab-cont-box .tab-inner-cont")[1].TextContent,
                    })
                    .FirstOrDefault();

                return jobDetail;
            }
        }

        public string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");
            strText = strText.Replace("\n", "");//删除\n
            strText = strText.Replace("\t", "");//删除\t

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }
        #endregion
    }
}
