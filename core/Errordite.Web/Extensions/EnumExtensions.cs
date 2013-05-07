using System;
using System.Collections.Generic;
using System.Resources;
using System.Web.Mvc;

namespace Errordite.Web.Extensions
{
    public static class EnumExtensions
    {
        public static IList<SelectListItem> ToSelectedList(this Enum type, ResourceManager resources, bool addOtherOption = false, string selectedValue = null)
        {
            Array enumValues = Enum.GetValues(type.GetType());
            string[] enumNames = Enum.GetNames(type.GetType());
            string resourceItemPrefix = type.GetType().Name + "_";

            IList<SelectListItem> dropdownItems = new List<SelectListItem>(enumNames.Length + (addOtherOption ? 1 : 0));

            if (addOtherOption)
            {
                // todo - add default to resources
                dropdownItems.Add(new SelectListItem { Selected = true, Text = "Please Select", Value = string.Empty });
            }

            for (int index = 0; index < enumValues.Length; index++)
            {
                string text = enumNames[index];
                if (resources != null)
                {
                    text = resources.GetString(resourceItemPrefix + text) ?? text;
                }

                var value = enumValues.GetValue(index).ToString();
                dropdownItems.Add(new SelectListItem
                {
                    //check value
                    Selected = selectedValue != null && value == selectedValue,
                    Text = text,
                    Value = value
                });
            }

            return dropdownItems;
        }
    }
}