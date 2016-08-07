using System;
using System.Linq.Expressions;
using System.Resources;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq;
using Errordite.Core.Extensions;

namespace Errordite.Core.Web
{
    public static class HtmlExtensions
    {
         public static MvcHtmlString DropDownListForEnum<T, TProperty>(this HtmlHelper<T> html, Expression<Func<T, TProperty>> expression, ResourceManager resourceManager, string defaultText = null, string defaultValue = null, object htmlAttributes = null, bool sortByLabel = false)
         {
             var enumType = typeof (TProperty);
             var selectListItems = Enum.GetNames(enumType).Select(
                 n => new SelectListItem
                    {
                        Text = resourceManager.GetString("{0}_{1}".FormatWith(enumType.Name, n)),
                        Value = n,
                        Selected = html.ViewData.Model != null && n == expression.Compile()(html.ViewData.Model).ToString()
                    });

             if (sortByLabel)
                 selectListItems = selectListItems.OrderBy(s => s.Text);

             var selectListList = selectListItems.ToList();

             if (defaultText != null && defaultValue != null)
             {
                selectListList.Insert(0, new SelectListItem
                {
                    Text = defaultText,
                    Value = defaultValue,
                    Selected = !selectListList.Any(li => li.Selected)
                });
             }

             return html.DropDownListFor(expression, selectListList, htmlAttributes);
         }
    }
}