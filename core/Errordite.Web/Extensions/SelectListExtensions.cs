using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using CodeTrip.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Web.Extensions
{
    public static class SelectListExtensions
    {
        public static List<SelectListItem> Empty(string defaultText = null, string defaultValue = null)
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Text = defaultText ?? "Please Select", Value = defaultValue ?? "0"}
            };
        }

        public static SelectListItem FindItemByValue(this IList<SelectListItem> list, string value)
        {
            return list.FirstOrDefault(selectListItem => selectListItem.Value == value);
        }

        public static List<SelectListItem> EnumToSelectList<TSource> (this TSource e, string unselectedText = null) 
            where TSource : struct
        {
            return ((TSource?) e).EnumToSelectList(unselectedText);
        }

        public static List<SelectListItem> EnumToSelectList<TSource> (this TSource? e, string unselectedText = null) 
            where TSource : struct
        {
            var type = typeof (TSource);
            var enumMembers = type.GetFields(BindingFlags.Public | BindingFlags.Static); 
            var enumOptions =
                enumMembers.Select(
                    n =>
                        {
                            var friendlyNameAttribute = n.GetCustomAttributes(typeof (FriendlyNameAttribute), false).Cast<FriendlyNameAttribute>().FirstOrDefault();

                            var value = n.GetValue(e);

                            return new SelectListItem()
                                {
                                    Selected = value.Equals(e),
                                    Text = friendlyNameAttribute == null ? value.ToString() : friendlyNameAttribute.Name,
                                    Value = value.ToString()
                                };
                        });

            if (unselectedText != null)
            {
                enumOptions = new[] {new SelectListItem() {Value = "", Text = unselectedText}}
                    .Union(enumOptions);
            }

            return enumOptions.ToList();
        }

        public static List<SelectListItem>  ToSelectList<TSource>(this IEnumerable<TSource> collection, Func<TSource, object> value, Func<TSource, object> text, Func<TSource, bool> selected = null, string defaultText = null, string defaultValue = null, SortSelectListBy? sortListBy = null)
        {
            SelectListItem selectedItem = null;

            var selectList = new List<SelectListItem>(collection.Select(item =>
            {
                var selectItem = new SelectListItem
                {
                    Selected = selected != null && selected(item),
                    Text = text(item).ToString(),
                    Value = value(item).ToString()
                };

                if (selectItem.Selected)
                    selectedItem = selectItem;

                return selectItem;
            }));

            if (sortListBy.HasValue)
            {
                selectList = selectList.OrderBy(item => sortListBy == SortSelectListBy.Text ? item.Text : item.Value).ToList();
            }

            if (!defaultText.IsNullOrEmpty())
            {
                selectList.Insert(0, new SelectListItem
                {
                    Text = defaultText,
                    Value = defaultValue ?? "0",
                    Selected = selectedItem == null,

                });
            }

            return selectList;
        }

        public static List<SelectListItem> GetRuleProperties(this ErrorditeConfiguration configuration, string selectedValue)
        {
            var selectList = new List<SelectListItem>(configuration.ErrorPropertiesForFiltering.Select(property => new SelectListItem
            {
                Value = property,
                Text = Resources.Rules.ResourceManager.GetString("ErrorProperty_{0}".FormatWith(property)),
                Selected = property == selectedValue
            }));

            selectList.Insert(0, new SelectListItem
            {
                Text = Resources.Shared.PleaseSelect,
                Value = Resources.Shared.DefaultSelectValue
            });

            return selectList;
        }
    }

    public enum SortSelectListBy
    {
        Text,
        Value
    }
}