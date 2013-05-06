using System;
using System.IO;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Errordite.Client;
using Newtonsoft.Json;

namespace Errordite.Receive.Binders
{
    public class ClientErrorModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var contentType = controllerContext.HttpContext.Request.ContentType;

            if (contentType.Trim().ToLowerInvariant().StartsWith("application/xml", StringComparison.OrdinalIgnoreCase))
            {
                string requestBody = GetRequestBody(controllerContext);

                if (string.IsNullOrEmpty(requestBody))
                    return base.BindModel(controllerContext, bindingContext);

                return DeserializeXml(requestBody);
            }

			var jsonBody = GetRequestBody(controllerContext);

			if (string.IsNullOrEmpty(jsonBody))
				return base.BindModel(controllerContext, bindingContext);

			return JsonConvert.DeserializeObject<ClientError>(jsonBody);
        }

        private string GetRequestBody(ControllerContext controllerContext)
        {
            string requestBody;

            using (var stream = controllerContext.HttpContext.Request.InputStream)
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream))
                    requestBody = reader.ReadToEnd();
            }

            return requestBody;
        }

        public static ClientError DeserializeXml(string xml)
        {
            using (var xmlTextReader = new XmlTextReader(new StringReader(xml)))
            {
                //there's a bit of funky stuff going on here. Let me explain:
                
                //The ClientError object model in Errordite.Client.Abstractions has different versions 
                //depending on whether we are in .net v2 or v3.5.  In v2, the ExceptionInfo.Data 
                //is of type List<ErrorDataItem>, but in v3.5 it is a Dictionary<string, string>.

                //We prefer the dictionary for json serialisation because it does the sensible thing
                //and converts it to a hash.

                //The upshot, though, is that if we are referencing the more modern Abstractions dll
                //here, it is not going to be able to deserialise directly to the dictionary property 
                //(because the XmlSerialzer can't do it - that's why we aren't using the dictionary in the first place).

                //So we deserialize instead to some shim classes, that inherit from the actual classes
                //but hide the properties that need to be different.
                //We could just copy-and-paste all the properties from Abstractions but then we'd lose 
                //cohesion and compile-time checking, so instead we need to tell the serialiser to 
                //ignore the properties on the base classes, and just deserialise to the hiding members
                //instead.

                //Whew - we got there!
                var overrides = new XmlAttributeOverrides();
                overrides.Add(typeof(ExceptionInfo), "Data", new XmlAttributes(){XmlIgnore = true});
                overrides.Add(typeof(ClientError), "ExceptionInfo", new XmlAttributes() { XmlIgnore = true });
                var xmlSerializer = new XmlSerializer(typeof(ClientErrorShim), overrides);
                var ret = (ClientError)xmlSerializer.Deserialize(xmlTextReader);
                return ret;
            }
        }
    }

    [XmlRoot("ClientError")]
    public class ClientErrorShim : ClientError
    {
        public new ExceptionInfoShim ExceptionInfo
        {
            get { throw new InvalidOperationException("Getter should not be called."); }
            set { base.ExceptionInfo = value; }
        }
    }

    public class ExceptionInfoShim : ExceptionInfo
    {
        public new ErrorDataItem[] Data
        {
            get { throw new InvalidOperationException("Getter should not be called.");}
            set
            {
                base.Data = new ErrorData();
                foreach (var kvp in value) base.Data.Add(kvp.Key, kvp.Value);
            }
        }
    }
}