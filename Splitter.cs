namespace Logger
{
    /// <summary>
    /// Specifies the visual separator used between the date/time and the log message.
    /// </summary>
    public enum Splitter
    {
        /// <summary>
        /// [DATE] MESSAGE
        /// </summary>
        Space = 0,

        /// <summary>
        /// [DATE] * MESSAGE
        /// </summary>
        Asterisk = 1,

        /// <summary>
        /// [DATE] | MESSAGE
        /// </summary>
        Pipe = 2,

        /// <summary>
        /// [DATE] - MESSAGE
        /// </summary>
        Dash = 3,

        /// <summary>
        /// [DATE] : MESSAGE
        /// </summary>
        Colon = 4,

        /// <summary>
        /// [DATE]	MESSAGE (Tab character)
        /// </summary>
        Tab = 5,

        /// <summary>
        /// [DATE] > MESSAGE
        /// </summary>
        Arrow = 6,

        /// <summary>
        /// [DATE] ~ MESSAGE
        /// </summary>
        Tilde = 7
    }
}