using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Errordite.Core.Configuration;
using Errordite.Core.Notifications.Sending;
using HtmlAgilityPack;

namespace Errordite.Core.Notifications.Rendering
{
    /// <summary>
    /// Takes email info and renders it into a message object ready for sending.
    /// </summary>
    /// <remarks>
    /// Partial class so that the nested "Directive" classes can live in another 
    /// </remarks>
    public partial class EmailRenderer : IEmailRenderer
    {
        private readonly EmailConfiguration _config;
        private readonly ITemplateLocator _templateLocator;

        /// <summary>
        /// Public ctor.
        /// </summary>
        public EmailRenderer(EmailConfiguration config, ITemplateLocator templateLocator)
        {
            _config = config;
            _templateLocator = templateLocator;
        }

        private EmailRenderer(EmailConfiguration config)
        {
            _config = config;
        }

        private StringBuilder _outputSb;
        private readonly StringBuilder _bufferSb = new StringBuilder();
        private readonly StringBuilder _harvestingSb = new StringBuilder();
        private StringBuilder _finalSb = new StringBuilder();
        private readonly Stack<Conditional> _conditionals = new Stack<Conditional>();
        private readonly Dictionary<string, StringBuilder> _placeholderSbs = new Dictionary<string, StringBuilder>();

        private void ChangeToPlaceholder(string placeholderName)
        {
            if (!_placeholderSbs.ContainsKey(placeholderName))
                _placeholderSbs.Add(placeholderName, new StringBuilder());

            _outputSb = _placeholderSbs[placeholderName];
            _finalSb = _outputSb;  //CONSIDER!
        }

        public Message Render(string template, IDictionary<string, string> emailParams)
        {
            string body = RenderFromTemplate(template, emailParams);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);

            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            string subject;
            if (titleNode != null)
            {
                subject = HttpUtility.HtmlDecode(titleNode.InnerText.Trim());
            }
            else
            {
                subject = emailParams.TryGetValue("Subject", out subject) ? subject : null;
            }

            
            string to = emailParams.TryGetValue("To", out to) ? to : "NOONE";
            string cc = emailParams.TryGetValue("Cc", out cc) ? cc : null;
            string bcc = emailParams.TryGetValue("Bcc", out bcc) ? bcc : null;

            string replyTo = emailParams.TryGetValue("ReplyTo", out replyTo) ? replyTo : null;

            return new Message
            {
                Body = body,
                Subject = subject,
                To = to,
                ReplyTo = replyTo,
                Bcc = bcc,
                Cc = cc
            };
        }
        public string RenderFromTemplate(string template, IDictionary<string, string> emailParams)
        {
            _state = EmailRendererState.Normal;
            _emailParams = emailParams;
            _outputSb = _finalSb;

            foreach (var c in template)
            {
                switch (_state)
                {
                    case EmailRendererState.Normal:
                        if (c == '$')
                        {
                            _outputSb.Append(_bufferSb.ToString());
                            _bufferSb.Clear();
                            _bufferSb.Append(c);
                            _state = EmailRendererState.BeginParam;
                        }
                        else
                        {
                            _bufferSb.Append(c);
                        }
                        break;
                    case EmailRendererState.BeginParam:
                        if (c == '(')
                        {
                            _bufferSb.Clear();
                            _state = EmailRendererState.InParam;
                        }
                        else
                        {
                            _state = EmailRendererState.Normal;
                        }
                        break;
                    case EmailRendererState.InParam:
                        if (c == ')')
                        {
                            var directive = GetDirective();
                            directive.MutateState(this);
                            _bufferSb.Clear();
                            _state = EmailRendererState.Normal;
                        }
                        else
                            _bufferSb.Append(c);
                        break;
                }
            }

            _outputSb.Append(_bufferSb.ToString());

            if (_masterTemplateName != null)
            {
                var masterRenderer = new EmailRenderer(_config, _templateLocator);
                var emailParamsCopy = emailParams.ToDictionary(x => x.Key, x => x.Value);

                foreach(var placeholder in _placeholderSbs)
                    emailParamsCopy[placeholder.Key] = placeholder.Value.ToString();

                if (!_placeholderSbs.Any())
                    emailParamsCopy["Placeholder"] = _finalSb.ToString();

                var masterTemplate = _templateLocator.GetTemplate(_masterTemplateName);
                return masterRenderer.RenderFromTemplate(masterTemplate, emailParamsCopy);
            }

            return _outputSb.ToString();
        }

        private EmailRendererState _state;
        private string _masterTemplateName;
        private string _iteratorBlock;
        private IDictionary<string, string> _emailParams;
    }
}