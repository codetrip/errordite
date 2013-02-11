namespace CodeTrip.Core.Auditing.Entities
{
    /// <summary>
    /// This extends an object to support auditing
    /// </summary>
    public interface ISupportsAuditing
    {
        IComponentAuditor Auditor { get; set; }
    }
}