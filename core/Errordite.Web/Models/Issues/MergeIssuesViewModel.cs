
namespace Errordite.Web.Models.Issues
{
    public class MergeIssuesViewModel
    {
        public string LeftIssueName { get; set; }
        public string LeftIssueStatus { get; set; }
        public string LeftIssueId { get; set; }

        public string RightIssueName { get; set; }
        public string RightIssueStatus { get; set; }
        public string RightIssueId { get; set; }
    }

    public class MergeIssuesPostModel
    {
        public string MergeToId { get; set; }
        public string MergeFromId { get; set; }
    }
}