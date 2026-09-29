namespace UrlMatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A URL pattern parsed once and reusable across any number of matches.
    /// Parse a pattern once at startup (for example, when a route is registered) and pass it to Matcher.Match
    /// to avoid re-parsing the pattern on every request, and to surface invalid patterns immediately.
    /// Instances are immutable after construction and are safe to use from multiple threads concurrently.
    /// </summary>
    /// <remarks>
    /// Pattern syntax, evaluated per segment after splitting on '/' and discarding empty segments:
    /// <list type="bullet">
    /// <item><description>{name} captures exactly one URL segment as name.</description></item>
    /// <item><description>{*name} is a catch-all.  It must be the entire final segment, matches zero or more remaining URL segments, and captures the raw remainder of the URL (empty string when nothing remains).</description></item>
    /// <item><description>{}, {*}, an unclosed brace such as {id, and segments without braces such as * or ** are literals.</description></item>
    /// <item><description>A segment containing a parameter group plus other text, such as v{version}, captures the whole URL segment as version.  Only the first brace group in a segment is considered.</description></item>
    /// </list>
    /// </remarks>
    public class UrlPattern
    {
        #region Public-Members

        /// <summary>
        /// The original pattern text.  Never null or empty.
        /// </summary>
        public string Pattern => _Pattern;

        /// <summary>
        /// The parsed segments, in order.  Empty for a pattern with no segments, such as /.
        /// </summary>
        public IReadOnlyList<UrlPatternSegment> Segments => _Segments;

        /// <summary>
        /// Total number of segments, including a catch-all segment if present.
        /// </summary>
        public int SegmentCount => _Segments.Count;

        /// <summary>
        /// Number of segments that must be matched one-to-one against URL segments, that is, every segment except a catch-all.
        /// </summary>
        public int FixedSegmentCount => _IsCatchAll ? _Segments.Count - 1 : _Segments.Count;

        /// <summary>
        /// Number of literal segments.
        /// </summary>
        public int LiteralCount => _LiteralCount;

        /// <summary>
        /// Number of single-segment parameters, written {name}.  A catch-all is not counted.
        /// </summary>
        public int ParameterCount => _ParameterCount;

        /// <summary>
        /// Number of consecutive literal segments at the start of the pattern.
        /// Useful when ranking routes, since a longer literal prefix is usually more specific.
        /// </summary>
        public int LiteralPrefixCount => _LiteralPrefixCount;

        /// <summary>
        /// True if the final segment is a catch-all, written {*name}.
        /// </summary>
        public bool IsCatchAll => _IsCatchAll;

        /// <summary>
        /// The catch-all parameter name, without braces or the asterisk.  Null when the pattern has no catch-all.
        /// </summary>
        public string CatchAllName => _IsCatchAll ? _Segments[_Segments.Count - 1].Name : null;

        #endregion

        #region Private-Members

        private readonly string _Pattern = null;
        private readonly ReadOnlyCollection<UrlPatternSegment> _Segments = null;
        private readonly int _LiteralCount = 0;
        private readonly int _ParameterCount = 0;
        private readonly int _LiteralPrefixCount = 0;
        private readonly bool _IsCatchAll = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object by parsing a pattern.
        /// </summary>
        /// <param name="pattern">The pattern, for example /{version}/users/{id} or /api/{*rest}.</param>
        /// <exception cref="ArgumentNullException">Thrown when pattern is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when a catch-all is not the entire segment, is not the last segment, or appears more than once.</exception>
        public UrlPattern(string pattern)
        {
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

            string[] parts = pattern.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            List<UrlPatternSegment> segments = new List<UrlPatternSegment>(parts.Length);

            int catchAllCount = 0;
            int catchAllIndex = -1;
            bool inPrefix = true;

            for (int i = 0; i < parts.Length; i++)
            {
                UrlPatternSegment segment = ParseSegment(pattern, parts[i]);
                segments.Add(segment);

                if (segment.Type == SegmentTypeEnum.Literal)
                {
                    _LiteralCount++;
                    if (inPrefix) _LiteralPrefixCount++;
                }
                else
                {
                    inPrefix = false;

                    if (segment.Type == SegmentTypeEnum.Parameter)
                    {
                        _ParameterCount++;
                    }
                    else
                    {
                        catchAllCount++;
                        if (catchAllIndex == -1) catchAllIndex = i;
                    }
                }
            }

            if (catchAllCount > 1)
                throw new ArgumentException(
                    "Pattern '" + pattern + "' contains " + catchAllCount + " catch-all segments; only one catch-all is allowed and it must be the last segment.",
                    nameof(pattern));

            if (catchAllCount == 1 && catchAllIndex != parts.Length - 1)
                throw new ArgumentException(
                    "Pattern '" + pattern + "' has catch-all segment '" + parts[catchAllIndex] + "' that is not the last segment; a catch-all must be the last segment.",
                    nameof(pattern));

            _Pattern = pattern;
            _Segments = segments.AsReadOnly();
            _IsCatchAll = catchAllCount == 1;
        }

        /// <summary>
        /// Parse a pattern.
        /// </summary>
        /// <param name="pattern">The pattern, for example /{version}/users/{id} or /api/{*rest}.</param>
        /// <returns>The parsed pattern.</returns>
        /// <exception cref="ArgumentNullException">Thrown when pattern is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when a catch-all is not the entire segment, is not the last segment, or appears more than once.</exception>
        public static UrlPattern Parse(string pattern)
        {
            return new UrlPattern(pattern);
        }

        /// <summary>
        /// Attempt to parse a pattern without throwing.
        /// </summary>
        /// <param name="pattern">The pattern, for example /{version}/users/{id} or /api/{*rest}.</param>
        /// <param name="result">The parsed pattern, or null when parsing fails.</param>
        /// <returns>True if the pattern is valid.  False if the pattern is null, empty, or has an invalid catch-all.</returns>
        public static bool TryParse(string pattern, out UrlPattern result)
        {
            result = null;
            if (String.IsNullOrEmpty(pattern)) return false;

            try
            {
                result = new UrlPattern(pattern);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return the original pattern text.
        /// </summary>
        /// <returns>Pattern text.</returns>
        public override string ToString()
        {
            return _Pattern;
        }

        #endregion

        #region Private-Methods

        private static UrlPatternSegment ParseSegment(string pattern, string text)
        {
            int indexStart = text.IndexOf('{');
            if (indexStart == -1) return new UrlPatternSegment(text, SegmentTypeEnum.Literal, null);

            int indexEnd = text.IndexOf('}', indexStart);
            if (indexEnd == -1 || indexEnd <= indexStart + 1) return new UrlPatternSegment(text, SegmentTypeEnum.Literal, null);

            string name = text.Substring(indexStart + 1, indexEnd - indexStart - 1);

            if (name[0] == '*')
            {
                // {*} has no name, so it is a literal, consistent with {}
                if (name.Length == 1) return new UrlPatternSegment(text, SegmentTypeEnum.Literal, null);

                if (indexStart != 0 || indexEnd != text.Length - 1)
                    throw new ArgumentException(
                        "Pattern '" + pattern + "' has catch-all '" + text.Substring(indexStart, indexEnd - indexStart + 1) + "' inside segment '" + text + "'; a catch-all must be the entire segment.",
                        nameof(pattern));

                return new UrlPatternSegment(text, SegmentTypeEnum.CatchAll, name.Substring(1));
            }

            return new UrlPatternSegment(text, SegmentTypeEnum.Parameter, name);
        }

        #endregion
    }
}
