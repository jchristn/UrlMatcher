namespace UrlMatcher
{
    using System;

    /// <summary>
    /// A single segment of a parsed URL pattern.
    /// Instances are immutable and safe to share across threads.
    /// </summary>
    public class UrlPatternSegment
    {
        #region Public-Members

        /// <summary>
        /// The segment text exactly as it appeared in the pattern, for example users, {id}, or {*rest}.
        /// Never null or empty.
        /// </summary>
        public string Text => _Text;

        /// <summary>
        /// The segment type.
        /// </summary>
        public SegmentTypeEnum Type => _Type;

        /// <summary>
        /// The parameter name for parameter and catch-all segments, without braces or the leading asterisk.
        /// Null for literal segments.
        /// </summary>
        public string Name => _Name;

        #endregion

        #region Private-Members

        private readonly string _Text = null;
        private readonly SegmentTypeEnum _Type = SegmentTypeEnum.Literal;
        private readonly string _Name = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="text">Segment text as it appeared in the pattern.</param>
        /// <param name="type">Segment type.</param>
        /// <param name="name">Parameter name.  Required for parameter and catch-all segments, must be null for literal segments.</param>
        /// <exception cref="ArgumentNullException">Thrown when text is null or empty, or when name is null or empty for a parameter or catch-all segment.</exception>
        /// <exception cref="ArgumentException">Thrown when a name is supplied for a literal segment.</exception>
        public UrlPatternSegment(string text, SegmentTypeEnum type, string name)
        {
            if (String.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (type == SegmentTypeEnum.Literal && name != null)
                throw new ArgumentException("Literal segment '" + text + "' cannot have a parameter name.", nameof(name));
            if (type != SegmentTypeEnum.Literal && String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "Segment '" + text + "' of type " + type.ToString() + " requires a parameter name.");

            _Text = text;
            _Type = type;
            _Name = name;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return the segment text.
        /// </summary>
        /// <returns>Segment text.</returns>
        public override string ToString()
        {
            return _Text;
        }

        #endregion
    }
}
