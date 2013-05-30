using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Errordite.Web.ActionSelectors
{
    /// <summary>
    /// Attribute for Controller methods to decide whether a particular button
    /// was clicked and hence whether the method can handle the action.
    /// </summary>
    public class IfButtonClickedAttribute : ActionMethodSelectorAttribute
    {
        private readonly IEnumerable<string> _buttonNames;

        public IfButtonClickedAttribute(params string[] buttonNames)
        {
            _buttonNames = buttonNames;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            if (controllerContext.HttpContext.Request.HttpMethod != "POST")
                return false;

            foreach (string buttonName in _buttonNames)
            {
                //this first test is for buttons or inputs that have the actual name specified
                if (controllerContext.HttpContext.Request.Form[buttonName] != null)
                    return true;

                //this second test is for inputs whose "name" was encoded into their name
                //using TescoSubmitInput
                if (controllerContext.HttpContext.Request.Form.Keys.Cast<string>().Any(formFieldName => formFieldName.StartsWith("btn___" + buttonName)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}