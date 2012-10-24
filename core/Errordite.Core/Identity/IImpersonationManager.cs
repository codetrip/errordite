using System;

namespace Errordite.Core.Identity
{
    /// <summary>
    /// Impersonation allows a suitably privileged user to impersonate another user
    /// for the purposes of diagnostics and support.
    /// </summary>
    public interface IImpersonationManager
    {
        /// <summary>
        /// The current status of impersonation for the session.
        /// </summary>
        ImpersonationStatus CurrentStatus { get; }
        /// <summary>
        /// Start impersonation.
        /// </summary>
        /// <param name="user"></param>
        void Impersonate(ImpersonationStatus user);
        /// <summary>
        /// Stop impersonating.
        /// </summary>
        void StopImpersonating();
    }

    /// <summary>
    /// Represents the current impersonation status of a user.
    /// </summary>
    public class ImpersonationStatus
    {
        /// <summary>
        /// Are they currently impersonating someone?
        /// </summary>
        public bool Impersonating { get; set; }
        /// <summary>
        /// The id of the user being impersonated.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The organisation id of the user being impersonated.
        /// </summary>
        public string OrganisationId { get; set; }
        /// <summary>
        /// When the impersonation expires.
        /// </summary>
        public DateTime? ExpiryUtc { get; set; }
    }
}