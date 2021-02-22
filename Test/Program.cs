using System;
using System.Collections.Generic;
using UrlMatcher;

namespace Test
{
    class Program
    {
        static Matcher _Matcher = new Matcher();

        static void Main(string[] args)
        {
            // _Matcher.Logger = Console.WriteLine;

            while (true)
            {
                Console.WriteLine("Example pattern: /{version}/foo/{whatever}/bar");
                Console.Write("Pattern : ");
                string pattern = Console.ReadLine();
                if (String.IsNullOrEmpty(pattern)) break;

                Console.WriteLine("Example URL: /v1.0/foo/woohoo/bar");
                Console.Write("URL     : ");
                string url = Console.ReadLine();
                if (String.IsNullOrEmpty(url)) break;

                Dictionary<string, string> vals = null;
                bool success = _Matcher.Match(url, pattern, out vals);
                if (success)
                {
                    Console.WriteLine("Match");
                    if (vals != null && vals.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> val in vals)
                        {
                            Console.WriteLine("  " + val.Key + ": " + val.Value);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No match");
                }
            }
        }
    }
}
