using System;
using System.Diagnostics;
using Errordite.Client;
using Errordite.Core.Exceptions;
using NUnit.Framework;
using Errordite.Core.Extensions;

namespace Errordite.Test
{
    [TestFixture]
    public class GenerateErrors
    {
        [Test]
        public void Generate()
        {
            /*
            Asos Test Token: lEQRBa5MoMpKbNevGwE8Hg== 
            Marketplace Test Token: 4b4stuqfB7tVTZv4nYbXOw==
            Codetrip Test Token: s1485PvBqftyCIty72nINg==
            */
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    if (i%3 == 0)
                    {
                        throw new ArgumentException("No such value for parameter named Bob!, iteration:={0}".FormatWith(i));
                    }
                    if (i%3 == 1)
                    {
                        throw new InvalidOperationException("You cant do that to this thing!, iteration:={0}".FormatWith(i));
                    }
                    
                    throw new ErrorditeConfigurationException("You specified invalid configuration!, iteration:={0}".FormatWith(i));
                }
                catch (Exception e)
                {
                    ErrorditeClient.ReportException(e, false);
                }
            }   

            Console.WriteLine(watch.ElapsedMilliseconds + "ms");
        }
    }
}
