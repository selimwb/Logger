namespace Logger
{
    /// <summary>
    /// Internal class used to serialize and deserialize logger configuration.
    /// </summary>
    internal class Settings
    {
        public string? FileLocation { get; set; }
        public string? ErrorPrefix { get; set; }
        public string? WarningPrefix { get; set; }
        public DateFormat? DateFormat { get; set; }
        public Splitter? SplitterMode { get; set; }
        public TimeSpan? FlushInterval { get; set; }
        public long? MaxFileSizeBytes
        {
            get { return _maxFileSizeBytes; }
            set
            {
                if (value == 0 || value == null)
                    _maxFileSizeBytes = 0;
                else if (value < 4096)
                    _maxFileSizeBytes = 4096;
                else
                    _maxFileSizeBytes = (long)value;
            }
        }
        private long _maxFileSizeBytes;

        public Settings() { }

        public Settings(string fileLocation, string errorPrefix, string warningPrefix, DateFormat dateFormat, Splitter splitter, TimeSpan flushInterval, long maxFileSizeBytes)
        {
            FileLocation = fileLocation;
            ErrorPrefix = errorPrefix;
            WarningPrefix = warningPrefix;
            DateFormat = dateFormat;
            SplitterMode = splitter;
            FlushInterval = flushInterval;
            MaxFileSizeBytes = maxFileSizeBytes;
        }
    }
}