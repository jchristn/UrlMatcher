namespace UrlMatcher
{
    /// <summary>
    /// Type of a single pattern segment.
    /// </summary>
    public enum SegmentTypeEnum
    {
        /// <summary>
        /// Literal segment.  Must match the corresponding URL segment exactly (ordinal, case-sensitive).
        /// </summary>
        Literal = 0,

        /// <summary>
        /// Parameter segment, written {name}.  Captures exactly one URL segment.
        /// </summary>
        Parameter = 1,

        /// <summary>
        /// Catch-all segment, written {*name}.  Must be the final segment of the pattern.
        /// Captures zero or more remaining URL segments as the raw remainder of the URL.
        /// </summary>
        CatchAll = 2
    }
}
