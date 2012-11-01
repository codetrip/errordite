using System;

namespace CodeTrip.Core.Auditing.Entities
{
    [Serializable]
    public class Error
    {
        public int ErrorId { get; set; }
        public string Component { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExtraData { get; set; }
        public string ExceptionText { get; set; }
        public DateTime TimestampUtc { get; set; }
        public int EventId { get; set; }
        public string MachineName { get; set; }
        public string ClientIpAddress { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public ErrorStatus Status { get; set; }
        public string PublishJobId { get; set; }
    }

    [Serializable]
    public enum ErrorStatus
    {
        New,
        UnderInvestigation,
        BugRaised,
        CannotReproduce,
        ImpossibleToPrevent,
        Fixed
    }
}