using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using GetSomeInput;
using UrlMatcher;

namespace Test
{
    class Program
    {
        static Matcher _Matcher = new Matcher();

        static void Main(string[] args)
        {
            _Matcher.Logger = Console.WriteLine;

            while (true)
            {
                string pattern = Inputty.GetString("Pattern :", "/{version}/foo/{whatever}/bar", false);
                string url =     Inputty.GetString("URL     :", "/v1.0/foo/foobar/bar", false);

                NameValueCollection vals = null;
                bool success = _Matcher.Match(url, pattern, out vals);
                if (success)
                {
                    Console.WriteLine("Match");
                    if (vals != null && vals.Count > 0)
                    {
                        for (int i = 0; i < vals.Count; i++)
                        {
                            Console.WriteLine("  " + vals.GetKey(i) + ": " + vals.Get(i));
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
