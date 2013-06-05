using Errordite.Core.Domain.Organisation;
using Errordite.Core.Interfaces;
using Errordite.Core.Session;
using System.Linq;

namespace Errordite.Core.Organisations.Commands
{
    public interface ISetExternallyBilledSubscriptionCommand : ICommand<SetExternallyBilledSubscriptionRequest, SetExternallyBilledSubscriptionResponse>
    {
    }

    public class SetExternallyBilledSubscriptionCommand : SessionAccessBase, ISetExternallyBilledSubscriptionCommand
    {
        public SetExternallyBilledSubscriptionResponse Invoke(SetExternallyBilledSubscriptionRequest request)
        {
            var plan = Session.MasterRaven.Query<PaymentPlan>()
                              .FirstOrDefault(p => p.Type == request.PlanType && p.SpecialId == request.ExternalPlanId);

            if (plan == null)
                return new SetExternallyBilledSubscriptionResponse(){Status = SetExternallyBilledSubscriptionStatus.PlanNotFound};

            var organisation = Session.MasterRaven.Load<Organisation>(Organisation.GetId(request.OrganisationId));

            organisation.PaymentPlanId = plan.Id;

            return new SetExternallyBilledSubscriptionResponse(){Status = SetExternallyBilledSubscriptionStatus.Ok};
        }
    }

    public enum SetExternallyBilledSubscriptionStatus
    {
        Ok,
        PlanNotFound
    }

    public class SetExternallyBilledSubscriptionRequest
    {
        public PaymentPlanType PlanType { get; set; }

        public string ExternalPlanId { get; set; }

        public string OrganisationId { get; set; }
    }

    public class SetExternallyBilledSubscriptionResponse
    {
        public SetExternallyBilledSubscriptionStatus Status { get; set; }
    }

}