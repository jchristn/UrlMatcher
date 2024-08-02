namespace UrlMatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// URL matcher.
    /// </summary>
    public class Matcher
    {
        #region Public-Members

        /// <summary>
        /// URL.
        /// </summary>
        public string Url
        {
            get
            {
                return _Url;
            }
        }

        /// <summary>
        /// URL parts.
        /// </summary>
        public string[] Parts
        {
            get
            {
                return _Parts;
            }
        }

        #endregion

        #region Private-Members

        private string _Url = null;
        private string[] _Parts = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="url">URL.</param>
        public Matcher(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            url = url.Split('?', '#')[0];

            _Url = url;
            _Parts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="uri">URI.</param>
        public Matcher(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            string url = uri.PathAndQuery.Split('?', '#')[0];

            _Url = url;
            _Parts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Match the URL or URI supplied in the constructor against a pattern.
        /// For example, match URI http://localhost:8000/v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// Or, match URL /v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// </summary>
        /// <param name="pattern">The pattern used to evaluate the URI.</param>
        /// <param name="vals">Name value collection containing keys and values.</param>
        /// <returns>True if matched.</returns>
        public bool Match(string pattern, out NameValueCollection vals)
        {
            vals = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

            string[] patternParts = pattern.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return MatchInternal(_Parts, patternParts, out vals);
        }

        /// <summary>
        /// Match a URI against a pattern.
        /// For example, match URI http://localhost:8000/v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// </summary>
        /// <param name="uri">The URI to evaluate.</param>
        /// <param name="pattern">The pattern used to evaluate the URI.</param>
        /// <param name="vals">Name value collection containing keys and values.</param>
        /// <returns>True if matched.</returns>
        public static bool Match(Uri uri, string pattern, out NameValueCollection vals)
        {
            vals = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

            return Match(uri.PathAndQuery, pattern, out vals);
        }

        /// <summary>
        /// Match a URL against a pattern.
        /// For example, match URL /v1.0/something/else/32 against pattern /{v}/something/else/{id}.
        /// If a match exists, vals will contain keys name 'v' and 'id', and the associated values from the supplied URL.
        /// </summary>
        /// <param name="url">The URL to evaluate.</param>
        /// <param name="pattern">The pattern used to evaluate the URL.</param>
        /// <param name="vals">Name value collection containing keys and values.</param>
        /// <returns>True if matched.</returns>
        public static bool Match(string url, string pattern, out NameValueCollection vals)
        {
            vals = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

            url = url.Split('?', '#')[0];

            string[] urlParts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] patternParts = pattern.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return MatchInternal(urlParts, patternParts, out vals);
        }

        #endregion

        #region Private-Methods

        private static bool MatchInternal(string[] urlParts, string[] patternParts, out NameValueCollection vals)
        {
            vals = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            if (urlParts.Length != patternParts.Length) return false;

            for (int i = 0; i < urlParts.Length; i++)
            {
                string paramName = ExtractParameter(patternParts[i]);

                if (String.IsNullOrEmpty(paramName))
                {
                    // no pattern
                    if (!urlParts[i].Equals(patternParts[i]))
                    {
                        vals = null;
                        return false;
                    }
                }
                else
                {
                    vals.Add(
                        paramName.Replace("{", "").Replace("}", ""),
                        urlParts[i]);
                }
            }

            return true;
        }

        private static string ExtractParameter(string pattern)
        {
            if (String.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

            if (pattern.Contains("{"))
            {
                if (pattern.Contains("}"))
                {
                    int indexStart = pattern.IndexOf('{');
                    int indexEnd = pattern.LastIndexOf('}');
                    if ((indexEnd - 1) > indexStart)
                    {
                        return pattern.Substring(indexStart, (indexEnd - indexStart + 1));
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
