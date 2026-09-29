namespace UrlMatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// URL matcher.
    /// Use the static methods to match a URL against a pattern once, or create an instance to parse a URL once and
    /// match it against many patterns.  Instances are immutable after construction and are safe to use from
    /// multiple threads concurrently.
    /// </summary>
    /// <remarks>
    /// URLs and patterns are split on '/' and empty segments are discarded, so leading, trailing, and repeated
    /// slashes do not affect matching.  The query string and fragment are removed from the URL (not the pattern)
    /// before matching.  Literal segments are compared ordinally (case-sensitive).  Parameter names are
    /// case-insensitive.  Values are returned exactly as they appear in the URL and are not URL-decoded.
    /// See <see cref="UrlPattern"/> for the full pattern syntax, including catch-all segments written {*name}.
    /// </remarks>
    public class Matcher
    {
        #region Public-Members

        /// <summary>
        /// URL, with the query string and fragment removed.
        /// </summary>
        public string Url => _Url;

        /// <summary>
        /// URL parts.
        /// Returns a copy of the internal array to prevent external modification.
        /// </summary>
        public string[] Parts => (string[])_Parts.Clone();

        #endregion

        #region Private-Members

        private string _Url = null;
        private string[] _Parts = null;
        private int[] _Offsets = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <exception cref="ArgumentNullException">Thrown when url is null or empty.</exception>
        public Matcher(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            _Url = StripQueryAndFragment(url);
            _Parts = SplitUrl(_Url, out _Offsets);
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="uri">URI.  Only the path is used.</param>
        /// <exception cref="ArgumentNullException">Thrown when uri is null.</exception>
        public Matcher(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            _Url = StripQueryAndFragment(uri.PathAndQuery);
            _Parts = SplitUrl(_Url, out _Offsets);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Match the URL or URI supplied in the constructor against a pattern.
        /// For example, match URI http://localhost:8000/v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// Or, match URL /v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// A pattern ending in a catch-all, such as /api/{*rest}, matches zero or more remaining segments and captures the raw remainder.
        /// </summary>
        /// <param name="pattern">The pattern used to evaluate the URI. Parameters are specified using {name} syntax, and a final catch-all using {*name} syntax.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match. URL-encoded values are matched as-is (not decoded).</param>
        /// <returns>True if matched. Note: Literal parts are case-sensitive while parameter names are case-insensitive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when pattern is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the pattern has an invalid catch-all (not the entire segment, not the last segment, or more than one).</exception>
        public bool Match(string pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));
            return MatchInternal(_Url, _Parts, _Offsets, new UrlPattern(pattern), out vals);
        }

        /// <summary>
        /// Match the URL or URI supplied in the constructor against a pre-parsed pattern.
        /// </summary>
        /// <param name="pattern">The parsed pattern.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match. URL-encoded values are matched as-is (not decoded).</param>
        /// <returns>True if matched.</returns>
        /// <exception cref="ArgumentNullException">Thrown when pattern is null.</exception>
        public bool Match(UrlPattern pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            return MatchInternal(_Url, _Parts, _Offsets, pattern, out vals);
        }

        /// <summary>
        /// Match a URI against a pattern.
        /// For example, match URI http://localhost:8000/v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// </summary>
        /// <param name="uri">The URI to evaluate.  Only the path is used.</param>
        /// <param name="pattern">The pattern used to evaluate the URI. Parameters are specified using {name} syntax, and a final catch-all using {*name} syntax.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match.</param>
        /// <returns>True if matched. Note: Literal parts are case-sensitive while parameter names are case-insensitive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when uri is null, or pattern is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the pattern has an invalid catch-all (not the entire segment, not the last segment, or more than one).</exception>
        public static bool Match(Uri uri, string pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));
            return Match(uri.PathAndQuery, new UrlPattern(pattern), out vals);
        }

        /// <summary>
        /// Match a URI against a pre-parsed pattern.
        /// </summary>
        /// <param name="uri">The URI to evaluate.  Only the path is used.</param>
        /// <param name="pattern">The parsed pattern.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match.</param>
        /// <returns>True if matched.</returns>
        /// <exception cref="ArgumentNullException">Thrown when uri or pattern is null.</exception>
        public static bool Match(Uri uri, UrlPattern pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            return Match(uri.PathAndQuery, pattern, out vals);
        }

        /// <summary>
        /// Match a URL against a pattern.
        /// For example, match URL /v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// A pattern ending in a catch-all, such as /api/{*rest}, matches zero or more remaining segments and captures the raw remainder.
        /// </summary>
        /// <param name="url">The URL to evaluate.</param>
        /// <param name="pattern">The pattern used to evaluate the URL. Parameters are specified using {name} syntax, and a final catch-all using {*name} syntax.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match.</param>
        /// <returns>True if matched. Note: Literal parts are case-sensitive while parameter names are case-insensitive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when url or pattern is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the pattern has an invalid catch-all (not the entire segment, not the last segment, or more than one).</exception>
        public static bool Match(string url, string pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));
            return Match(url, new UrlPattern(pattern), out vals);
        }

        /// <summary>
        /// Match a URL against a pre-parsed pattern.
        /// </summary>
        /// <param name="url">The URL to evaluate.</param>
        /// <param name="pattern">The parsed pattern.</param>
        /// <param name="vals">Name value collection containing keys and values. Parameter names are case-insensitive. Never null, and empty if no match.</param>
        /// <returns>True if matched.</returns>
        /// <exception cref="ArgumentNullException">Thrown when url is null or empty, or pattern is null.</exception>
        public static bool Match(string url, UrlPattern pattern, out NameValueCollection vals)
        {
            vals = NewCollection();
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            url = StripQueryAndFragment(url);
            string[] urlParts = SplitUrl(url, out int[] urlOffsets);
            return MatchInternal(url, urlParts, urlOffsets, pattern, out vals);
        }

        #endregion

        #region Private-Methods

        private static NameValueCollection NewCollection()
        {
            return new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        }

        private static bool MatchInternal(string url, string[] urlParts, int[] urlOffsets, UrlPattern pattern, out NameValueCollection vals)
        {
            vals = NewCollection();

            int fixedCount = pattern.FixedSegmentCount;

            if (pattern.IsCatchAll)
            {
                if (urlParts.Length < fixedCount) return false;
            }
            else
            {
                if (urlParts.Length != fixedCount) return false;
            }

            IReadOnlyList<UrlPatternSegment> segments = pattern.Segments;
            NameValueCollection captured = NewCollection();

            for (int i = 0; i < fixedCount; i++)
            {
                UrlPatternSegment segment = segments[i];

                if (segment.Type == SegmentTypeEnum.Literal)
                {
                    // literal match (case-sensitive)
                    if (!urlParts[i].Equals(segment.Text, StringComparison.Ordinal)) return false;
                }
                else
                {
                    captured.Add(segment.Name, urlParts[i]);
                }
            }

            if (pattern.IsCatchAll)
            {
                // raw remainder of the URL starting at the first unmatched segment, so repeated and trailing slashes are preserved
                string remainder = urlParts.Length > fixedCount ? url.Substring(urlOffsets[fixedCount]) : "";
                captured.Add(pattern.CatchAllName, remainder);
            }

            vals = captured;
            return true;
        }

        private static string[] SplitUrl(string url, out int[] offsets)
        {
            List<string> parts = new List<string>();
            List<int> starts = new List<int>();

            int i = 0;
            while (i < url.Length)
            {
                if (url[i] == '/')
                {
                    i++;
                    continue;
                }

                int start = i;
                while (i < url.Length && url[i] != '/') i++;

                parts.Add(url.Substring(start, i - start));
                starts.Add(start);
            }

            offsets = starts.ToArray();
            return parts.ToArray();
        }

        private static string StripQueryAndFragment(string url)
        {
            int queryIndex = url.IndexOf('?');
            int fragmentIndex = url.IndexOf('#');

            if (queryIndex == -1 && fragmentIndex == -1) return url;

            int cutIndex = queryIndex;
            if (cutIndex == -1 || (fragmentIndex != -1 && fragmentIndex < cutIndex))
            {
                cutIndex = fragmentIndex;
            }

            return url.Substring(0, cutIndex);
        }

        #endregion
    }
}
