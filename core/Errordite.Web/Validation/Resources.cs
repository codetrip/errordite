
namespace Errordite.Web.Validation
{
    public static class ValidationResources
    {
        public static class Regexes
        {
            public const string EmailAddress = @"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$";
            public const string TelephoneNumber = @"^[0-9\+\(\)\-\s]*$";
            public const string True = "^(True|true|1|on)$";
            public const string False = "^(False|false|0|off)$";
            public const string BetweenThreeAndNineCharacters = @"^.{3,9}$";
            public const string NumbersOnly = "^[0-9]+$";
            public const string LowercaseLettersOnly = "^[a-z]+$";
            public const string UppercaseLettersOnly = "^[A-Z]+$";
            public const string AlphaNumericsOnly = @"^([A-Z]|[a-z]|[0-9]|\s)+$";
            public const string NotEmptyGuid = @"^((?!00000000-0000-0000-0000-000000000000).)*$";
            public const string DoesNotContainWebAddress = @"^((?!((https?\:\/\/)?([\w\d\-]+\.){2,}([\w\d]{2,})((\/[\w\d\-\.]+)*(\/[\w\d\-]+\.[\w\d]{3,4}(\?.*)?)?)?)).)*$";
            public const string WebAddress = @"((https?\:\/\/)?([\w\d\-]+\.){2,}([\w\d]{2,})((\/[\w\d\-\.]+)*(\/[\w\d\-]+\.[\w\d]{3,4}(\?.*)?)?)?)";
        }
    }
}