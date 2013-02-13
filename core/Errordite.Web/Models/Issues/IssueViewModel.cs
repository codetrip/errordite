using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Errordite.Core.Domain.Error;
using Errordite.Web.Models.Errors;

namespace Errordite.Web.Models.Issues
{
    public class IssueViewModel
    {
        public IssueTab Tab { get; set; }
        public IssueRulesPostModel Rules { get; set; }
        public IssueDetailsViewModel Details { get; set; }
        public ErrorCriteriaViewModel Errors { get; set; }
    }

    public class IssueDetailsViewModel : IssueDetailsPostModel
    {
        public DateTime LastErrorUtc { get; set; }
        public DateTime FirstErrorUtc { get; set; }
        public int ErrorCount { get; set; }
        public string UserName { get; set; }
        public string ApplicationName { get; set; }
        public string ErrorLimitStatus { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public bool TestIssue { get; set; }
		public Error SampleError { get; set; }
    }

    public class IssueDetailsPostModel
    {
        public string Comment { get; set; }
        public string Reference { get; set; }
        public string IssueId { get; set; }
        [Required(ErrorMessage = "Please enter a name")]
        public string Name { get; set; }
        public string UserId { get; set; }
        public IssueStatus Status { get; set; }
        public bool AlwaysNotify { get; set; }
        public string DateRange { get; set; }
    }

	public class AddCommentViewModel
	{
		[Required(ErrorMessage = "Please enter a comment")]
		public string Comment { get; set; }
		public string IssueId { get; set; }
	}

    public class IssueHistoryItemViewModel
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public string UserEmail { get; set; }
        public DateTime DateAddedUtc { get; set; }
        public bool SystemMessage { get; set; }
        public string Reference { get; set; }
        public string IssueLink { get; set; }
    }

    public class IssueHistoryPostModel
    {
        public string IssueId { get; set; }
        [Required(ErrorMessageResourceType = typeof(Resources.History), ErrorMessageResourceName = "Message_Required")]
        [StringLength(500, MinimumLength = 5, ErrorMessageResourceType = typeof(Resources.History), ErrorMessageResourceName = "Message_Invalid_Length")]
        public string HistoryMessage { get; set; }
        public string Changeset { get; set; }
    }

    public enum IssueTab
    {
        Details,
        Reports,
        Rules,
        Errors,
        Debug,
        History,
		Actions
    }
}