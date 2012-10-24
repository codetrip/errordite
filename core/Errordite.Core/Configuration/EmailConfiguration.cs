
namespace Errordite.Core.Configuration
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpServerPort { get; set; }
        public string SmtpServerUsername { get; set; }
        public string SmtpServerPassword { get; set; }
        public bool IsSmtpSecureConnection { get; set; }
        public string TemplateLocation { get; set; }
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
        public string ErrorditeUrl { get; set; }
    }
}
