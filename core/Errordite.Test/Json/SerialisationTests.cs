using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Errordite.Core.Domain.Error;
using Errordite.Core.Matching;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Errordite.Test.Json
{
    [TestFixture]
    public class SerialisationTests
    {
         [Test]
        public void Test()
         {
             //var stream = new MemoryStream();
             var objectContent =
                 new ObjectContent<IssueBase>(new Issue(){Rules = new List<IMatchRule>(){new PropertyMatchRule()}}, new JsonMediaTypeFormatter(){SerializerSettings = new JsonSerializerSettings(){TypeNameHandling = TypeNameHandling.Objects}}).ReadAsStringAsync().
                     ContinueWith(t => Console.WriteLine(t.Result));
             //new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(IssueBase), new Issue(){Rules = new List<IMatchRule>(new PropertyMatchRule())}, stream, HttpContentHeaders., )
         }
    }
}