using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeTrip.Core.Exceptions;
using Errordite.Core.Domain.Error;
using CodeTrip.Core.Extensions;
using ProtoBuf;
using System.Linq;

namespace Errordite.Core.Matching
{
    [ProtoContract]
    public class PropertyMatchRule : MatchRule
    {
        [ProtoMember(1)]
        public string ErrorProperty { get; set; }
        [ProtoMember(2)]
        public StringOperator StringOperator { get; set; }
        [ProtoMember(3)]
        public string Value { get; set; }

        public PropertyMatchRule()
        {}

        public PropertyMatchRule(string errorPropertyName, StringOperator stringOperator, string value)
        {
            ErrorProperty = errorPropertyName;
            StringOperator = stringOperator;
            Value = value;
        }

        private string TreatValue(string value)
        {
            if (value == null)
                return null;
            value = value.ToLowerInvariant().Trim();
            //cope with the different (or at least likely) line endings we might hope by converting them
            //all to Windows endings (which the rule values will have)
            //regex from http://stackoverflow.com/a/8196219/3856
            return Regex.Replace(value, @"\r\n?|\n", Environment.NewLine);
        }

        public override bool IsMatch(Error error)
        {
            var ruleValue = TreatValue(Value);

            var valuesFromError = GetValuesFromError(error);

            if (ruleValue.IsNullOrEmpty())
            {
                return valuesFromError.Any(v => v.IsNullOrEmpty());
            }

            return valuesFromError.Any(v =>
            {
                if (v == null) return false;
                switch (StringOperator)
                {
                    case StringOperator.StartsWith:
                        return v.StartsWith(ruleValue);
                    case StringOperator.DoesNotEqual:
                        return v != ruleValue;
                    case StringOperator.Contains:
                        return v.Contains(ruleValue);
                    case StringOperator.DoesNotContain:
                        return !v.Contains(ruleValue);
                    case StringOperator.EndsWith:
                        return v.EndsWith(ruleValue);
                    case StringOperator.Equals:
                        return v == ruleValue;
                    case StringOperator.RegexMatches:
                        return Regex.IsMatch(v, ruleValue);
                    default: 
                        throw new CodeTripUnexpectedValueException("StringOperator", StringOperator.ToString());
                }
            });
        }

        private IEnumerable<string> GetValuesFromError(Error error)
        {
            var prop = typeof(ExceptionInfo).GetProperty(ErrorProperty);
            
            if(prop != null)
            {
                return error.ExceptionInfos.Select(i =>
                {
                    var value = prop.GetValue(i, null) as string;
                    return TreatValue(value);
                });
            }

            prop = typeof(Error).GetProperty(ErrorProperty);
            var otherValue = prop.GetValue(error, null) as string;
            return new[] { TreatValue(otherValue) };
        }

        public override string GetDescription()
        {
            return "{0} {1} {2}".FormatWith(ErrorProperty, StringOperator, Value);
        }
    }

    public enum StringOperator
    {
        Equals,
        DoesNotEqual,
        Contains,
        DoesNotContain,
        StartsWith,
        EndsWith,
        RegexMatches
    }
}