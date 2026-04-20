namespace Logger
{
    /// <summary>
    /// Defines the format in which the timestamp will be printed in the logs.
    /// </summary>
    public enum DateFormat
    {
        /// <summary>
        /// dd/MM/yyyy HH:mm:ss:fff (e.g., 20/04/2026 20:45:04:123)
        /// Full date, time, and milliseconds.
        /// </summary>
        DateTimeWithMilliseconds = 0,

        /// <summary>
        /// dd/MM/yyyy HH:mm:ss (e.g., 20/04/2026 20:45:04)
        /// Standard date and time.
        /// </summary>
        StandardDateTime = 1,

        /// <summary>
        /// dd/MM/yyyy (e.g., 20/04/2026)
        /// Day, month, and year only.
        /// </summary>
        ShortDate = 2,

        /// <summary>
        /// yyyy-MM-dd (e.g., 2026-04-20)
        /// ISO 8601 Standard. The safest format for database (SQL) operations and chronological sorting.
        /// </summary>
        Iso8601Date = 3,

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss (e.g., 2026-04-20 20:45:04)
        /// ISO 8601 Date and Time. A universal standard, highly useful for cross-system data transfer.
        /// </summary>
        Iso8601DateTime = 4,

        /// <summary>
        /// HH:mm:ss (e.g., 20:45:04)
        /// Time information only.
        /// </summary>
        TimeOnly = 5,

        /// <summary>
        /// HH:mm:ss:fff (e.g., 20:45:04:123)
        /// Time information only with milliseconds.
        /// </summary>
        TimeOnlyWithMilliseconds = 6,
    }
}