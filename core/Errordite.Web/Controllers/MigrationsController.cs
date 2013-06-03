using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;

namespace Errordite.Web.Controllers
{
    public class MigrationsController : ErrorditeController
    {
        //
        // GET: /Migrations/

        public ActionResult Index()
        {
            foreach (
                var method in
                    GetType().GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(m => m.ReturnType == typeof(ActionResult)))
            {
                Response.Write("<a href='{0}'>{1}</a><br/>".FormatWith(Url.Action(method.Name), method.Name));
            }

            return new EmptyResult();
        }

        public ActionResult FreeTier()
        {
            var plans = Core.Session.MasterRaven.Query<PaymentPlan>();

            foreach (var plan in plans)
            {
                if (plan.Price == 0m)
                {
                    plan.Name = PaymentPlanNames.Free;
                    plan.MaximumIssues = 15;
                }

                if (plan.Name == PaymentPlanNames.Large)
                {
                    plan.MaximumIssues = int.MaxValue;
                    plan.Price = 249.00m;
                }
            }

            return Content("OK");
        }

        public ActionResult AppHarborPlans()
        {
            var plans = Core.Session.MasterRaven.Query<PaymentPlan>();

            foreach (var plan in plans)
            {
                if (plan.Type == PaymentPlanType.Standard)
                    plan.Type = PaymentPlanType.Standard;
            }

            foreach (var paymentPlan in new[]
                {
                    GetAppHarborPaymentPlan("Free", 15, "free", 0, 1),
                    GetAppHarborPaymentPlan("Small", 50, "small", 19, 2),
                    GetAppHarborPaymentPlan("Medium", 79, "medium", 500, 3),
                    GetAppHarborPaymentPlan("Large", 249, "large", 249, int.MaxValue),
                })
            {
                if (!plans.Any(p => p.Type == PaymentPlanType.AppHarbor && p.SpecialId == paymentPlan.SpecialId))
                {
                    Core.Session.MasterRaven.Store(paymentPlan);
                }
            }

            return Content("done");

        }

        private PaymentPlan GetAppHarborPaymentPlan(string name, int maxIssues, string specialId, decimal price, int rank)
        {
            return new PaymentPlan()
                {
                    MaximumIssues = maxIssues,
                    Name = name,
                    Price = price, 
                    Rank = rank,
                    SpecialId = specialId,
                    Type = PaymentPlanType.AppHarbor,
                };
        }

    }
}
