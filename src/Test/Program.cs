namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using GetSomeInput;
    using UrlMatcher;

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string pattern = Inputty.GetString("Pattern :", "/{version}/foo/{whatever}/bar", false);
                string url =     Inputty.GetString("URL     :", "/v1.0/foo/foobar/bar", false);

                NameValueCollection vals = null;
                bool staticSuccess = Matcher.Match(url, pattern, out vals);
                if (staticSuccess)
                {
                    Console.WriteLine("| Match (static)");
                    if (vals != null && vals.Count > 0)
                    {
                        for (int i = 0; i < vals.Count; i++)
                        {
                            Console.WriteLine("  | " + vals.GetKey(i) + ": " + vals.Get(i));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No match (static)");
                }

                Matcher matcher = new Matcher(url);
                if (matcher.Match(pattern, out vals))
                {
                    Console.WriteLine("| Match (instance)");
                    if (vals != null && vals.Count > 0)
                    {
                        for (int i = 0; i < vals.Count; i++)
                        {
                            Console.WriteLine("  | " + vals.GetKey(i) + ": " + vals.Get(i));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No match (instance)");
                }
            }
        }
    }
}
