using System.Collections.Generic;

namespace JobCrawler.Models
{
    public class LagouResponseDTO
    {
        public bool Success { get; set; }

        public string ResubmitToken { get; set; }

        public string RequestId { get; set; }

        public string Msg { get; set; }

        public int Code { get; set; }

        public Content Content { get; set; }
    }

    public class Content
    {
        public int PageNo { get; set; }

        public int PageSize { get; set; }

        public object HrInfoMap { get; set; }

        public PositionResult PositionResult { get; set; }
    }

    public class PositionResult
    {
        public object LocationInfo { get; set; }

        public object QueryAnalysisInfo { get; set; }

        public object StrategyProperty { get; set; }

        public int TotalCount { get; set; }

        public int ResultSize { get; set; }

        public string HotLabels { get; set; }

        public Result[] Result { get; set; }
    }

    public class Result
    {
        public string ImState { get; set; }

        public string LastLogin { get; set; }

        public string Explain { get; set; }

        public int PcShow { get; set; }

        public int AppShow { get; set; }

        public int Deliver { get; set; }

        public string GradeDescription { get; set; }

        public string PromotionScoreExplain { get; set; }

        public string FirstType { get; set; }

        public string SecondType { get; set; }

        public int IsSchoolJob { get; set; }

        public string SubwayLine { get; set; }

        public string StationName { get; set; }

        public string LineStaion { get; set; }

        public string CreateTime { get; set; }

        public int CompanyId { get; set; }

        public string City { get; set; }

        public string Salary { get; set; }

        public string FinanceStage { get; set; }

        public int PositionId { get; set; }

        public string CompanyLogo { get; set; }

        public string PositionAdvantage { get; set; }

        public string PositionName { get; set; }

        public string WorkYear { get; set; }

        public string Education { get; set; }

        public string JobNature { get; set; }

        public string CompanyShortName { get; set; }

        public int Approve { get; set; }

        public string IndustryField { get; set; }

        public string District { get; set; }

        public int Score { get; set; }

        public List<string> CompanyLabelList { get; set; }

        public string CompanySize { get; set; }

        public int PublisherId { get; set; }

        public List<string> PositionLables { get; set; }

        public List<string> IndustryLables { get; set; }

        public List<string> BusinessZones { get; set; }

        public string FormatCreateTime { get; set; }

        public string CompanyFullName { get; set; }

        public int AdWord { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }
    }
}
