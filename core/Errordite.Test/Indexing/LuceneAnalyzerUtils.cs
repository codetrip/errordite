using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using CodeTrip.Core.Extensions;

namespace Errordite.Test.Indexing
{
    public class LuceneAnalyzerUtils
    {
        public static IEnumerable<string> TokensFromAnalysis(Analyzer analyzer, String text)
        {
            TokenStream stream = analyzer.TokenStream("contents", new StringReader(text));
            List<string> result = new List<string>();
            TermAttribute tokenAttr = (TermAttribute)stream.GetAttribute(typeof(TermAttribute));

            while (stream.IncrementToken())
            {
                Console.WriteLine("Buffer:={0}, Length:={1}, Term:={2}".FormatWith(tokenAttr.TermBuffer(), tokenAttr.TermLength(), tokenAttr.Term()));
                result.Add(tokenAttr.Term());
            }

             

            //tokenAttr.

            stream.End();
            stream.Close();

            return result;
        }
    }
}
