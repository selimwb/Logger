using System.Diagnostics;
using System.Threading.Channels;
using System.Text.Json;

namespace Logger
{
    /// <summary>
    /// A thread-safe, asynchronous singleton logger class that handles writing log messages to a file.
    /// </summary>
    public class Log : IDisposable
    {
        private static Log? _instance;
        private static readonly object _lock = new();

        private FileStream _stream;
        private Channel<string> _logChannel = Channel.CreateUnbounded<string>();
        private Task _worker;
        private Stopwatch _stopWatch = new();

        private Splitter _splitter;
        private DateFormat _dateFormat;
        private TimeSpan _flushInterval;

        private string _error;
        private string _warning;
        private string _fileLocation;
        private string _defaultLogLocation;
        private string _settingsLocation;

        private long _maxFileSizeBytes;

        /// <summary>
        /// Gets the prefix text applied to error logs.
        /// </summary>
        public string ErrorPrefix => _error;

        /// <summary>
        /// Gets the prefix text applied to warning logs.
        /// </summary>
        public string WarningPrefix => _warning;

        /// <summary>
        /// Gets the current splitter symbol used between the timestamp and the message.
        /// </summary>
        public Splitter LogSplitter => _splitter;

        /// <summary>
        /// Gets the current date formatting used in the logs.
        /// </summary>
        public DateFormat LogDateFormat => _dateFormat;

        /// <summary>
        /// Gets the interval at which the log buffer is flushed to the physical file.
        /// </summary>
        public TimeSpan FlushInterval => _flushInterval;

        /// <summary>
        /// Gets the full path of the current active log file.
        /// </summary>
        public string FileLocation => _fileLocation;

        /// <summary>
        /// Gets the default path where the log file is created initially within the AppData directory.
        /// </summary>
        public string DefaultLogLocation => _defaultLogLocation;

        /// <summary>
        /// Gets the full path where the logger configuration settings are stored in JSON format.
        /// </summary>
        public string SettingsLocation => _settingsLocation;

        /// <summary>
        /// Gets the maximum allowed file size in bytes before a log rotation occurs. Returns 0 if unlimited.
        /// </summary>
        public long MaxFileSizeBytes => _maxFileSizeBytes;

        private Log(string appName)
        {
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName);

            Directory.CreateDirectory(appDataDir);

            _settingsLocation = Path.Combine(appDataDir, "logSettings.dat");
            _defaultLogLocation = Path.Combine(appDataDir, "log.txt");

            ReadSettings();

            if (!File.Exists(_fileLocation))
            {
                File.WriteAllText(_fileLocation, "-- log file created by selim_wb --" + CreatePattern("Log file created."));
            }

            _stream = new FileStream(_fileLocation, FileMode.Append, FileAccess.Write, FileShare.Read);

            _stopWatch.Start();

            _worker = Task.Run(ProcessLogsAsync);
        }

        /// <summary>
        /// Gets the singleton instance of the Logger. Initializes a new instance if it doesn't exist.
        /// </summary>
        /// <param name="appName">The name of the application. Used to create a dedicated directory in the system's AppData folder for storing logs and settings.</param>
        /// <returns>The active Log instance.</returns>
        public static Log GetLogger(string appName)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new Log(appName);
                }
            }
#if DEBUG
            else if (_instance._settingsLocation.Contains(appName) == false)
            {
                Console.WriteLine($"[WARNING] Logger already initialized. '{appName}' ignored.");
            }
#endif

            return _instance;
        }

        /// <summary>
        /// Adds a standard informational log message to the queue.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public async Task AddLog(string logMessage)
        {
            await _logChannel.Writer.WriteAsync(CreatePattern(logMessage));
        }

        /// <summary>
        /// Adds a warning log message to the queue.
        /// </summary>
        /// <param name="logMessage">The warning message to log.</param>
        public async Task AddWarningLog(string logMessage)
        {
            await _logChannel.Writer.WriteAsync(CreatePattern($"[{_warning}]{AddSplitter(_splitter)}{logMessage}"));
        }

        /// <summary>
        /// Adds an error log message to the queue.
        /// </summary>
        /// <param name="logMessage">The error message to log.</param>
        public async Task AddErrorLog(string logMessage)
        {
            await _logChannel.Writer.WriteAsync(CreatePattern($"[{_error}]{AddSplitter(_splitter)}{logMessage}"));
        }

        private void ReadSettings()
        {
            if (File.Exists(_settingsLocation))
            {
                Settings? settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_settingsLocation));
                if (settings?.FileLocation != null &&
                    settings.FlushInterval != null &&
                    settings.SplitterMode != null &&
                    settings.ErrorPrefix != null &&
                    settings.DateFormat != null &&
                    settings.WarningPrefix != null &&
                    settings.MaxFileSizeBytes != null)
                {
                    _fileLocation = settings.FileLocation;
                    _dateFormat = (DateFormat)settings.DateFormat;
                    _flushInterval = (TimeSpan)settings.FlushInterval;
                    _splitter = (Splitter)settings.SplitterMode;
                    _error = settings.ErrorPrefix;
                    _warning = settings.WarningPrefix;
                    _maxFileSizeBytes = (long)settings.MaxFileSizeBytes;
                    return;
                }
            }

            _fileLocation = _defaultLogLocation;
            _flushInterval = TimeSpan.FromSeconds(3);
            _splitter = Splitter.Dash;
            _dateFormat = DateFormat.DateTimeWithMilliseconds;
            _error = "ERROR";
            _warning = "WARNING";
            _maxFileSizeBytes = 0;
            SaveSettings();
        }

        private void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsLocation)!);

            if (File.Exists(_settingsLocation))
            {
                File.Delete(_settingsLocation);
            }
            File.WriteAllText(_settingsLocation, JsonSerializer.Serialize<Settings>(new Settings(_fileLocation, _error, _warning, _dateFormat, _splitter, _flushInterval, _maxFileSizeBytes)));
        }

        /// <summary>
        /// Changes the interval at which the log buffer is flushed to the physical file.
        /// </summary>
        /// <param name="flushInterval">The interval in seconds.</param>
        public void ChangeFlushTime(int flushInterval)
        {
            _flushInterval = TimeSpan.FromSeconds(flushInterval);
            SaveSettings();
        }

        /// <summary>
        /// Disposes the logger, ensures the queue is processed, and flushes remaining data to the file.
        /// </summary>
        public void Dispose()
        {
            _logChannel.Writer.Complete();

            _worker.Wait();

            _stream.Flush();
            _stream.Dispose();
        }

        /// <summary>
        /// Returns a formatted string containing the current configuration details of the logger.
        /// </summary>
        /// <returns>A string representation of the logger's settings.</returns>
        public override string ToString()
        {
            return $"[Logger Info]\n" +
                   $"  App Data Directory   : {Path.GetDirectoryName(_settingsLocation)}\n" +
                   $"  Log File Location    : {_fileLocation}\n" +
                   $"  Date Format          : {_dateFormat} [{AddDate(_dateFormat)}]\n" +
                   $"  Splitter             : {_splitter} [{AddSplitter(_splitter)}]\n" +
                   $"  Flush Interval       : {_flushInterval.TotalSeconds} seconds\n" +
                   $"  Maximum File Size    : {(_maxFileSizeBytes == 0 ? "Unlimited" : $"{_maxFileSizeBytes / 1024 * 1024} MB")}\n" +
                   $"  Error Prefix         : [{_error}]\n" +
                   $"  Warning Prefix       : [{_warning}]";
        }

        private async Task ProcessLogsAsync()
        {
            try
            {
                await foreach (var log in _logChannel.Reader.ReadAllAsync())
                {
                    byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(log);
                    await _stream.WriteAsync(strBytes);

                    if (_maxFileSizeBytes != 0 && _stream.Length >= _maxFileSizeBytes)
                    {
                        await RotateFileAsync();
                    }

                    if (_stopWatch.Elapsed >= _flushInterval)
                    {
                        await _stream.FlushAsync();
                        _stopWatch.Restart();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
        }

        private async Task RotateFileAsync()
        {
            await _stream.FlushAsync();
            _stream.Dispose();

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            string dir = Path.GetDirectoryName(_fileLocation)!;
            string name = Path.GetFileNameWithoutExtension(_fileLocation);
            string ext = Path.GetExtension(_fileLocation);
            string archivePath = Path.Combine(dir, $"{name}_{timestamp}{ext}");

            File.Move(_fileLocation, archivePath);

            _stream = new FileStream(_fileLocation, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        private string CreatePattern(string text)
        {
            return $"{Environment.NewLine}[{AddDate(_dateFormat)}]{AddSplitter(_splitter)}{text}";
        }

        /// <summary>
        /// Changes the splitter symbol used between the timestamp and the message.
        /// </summary>
        /// <param name="splitter">The new splitter format.</param>
        public void ChangeSplitter(Splitter splitter)
        {
            try
            {
                _splitter = splitter;
                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Log Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Changes the date formatting of the logs.
        /// </summary>
        /// <param name="dateFormat">The new date format.</param>
        public void ChangeDateFormat(DateFormat dateFormat)
        {
            try
            {
                _dateFormat = dateFormat;
                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Changes the prefix text applied to error logs.
        /// </summary>
        /// <param name="errorText">The new error prefix text.</param>
        public void ChangeErrorText(string errorText)
        {
            try
            {
                _error = errorText;
                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Changes the prefix text applied to warning logs.
        /// </summary>
        /// <param name="warningText">The new warning prefix text.</param>
        public void ChangeWarningText(string warningText)
        {
            try
            {
                _warning = warningText;
                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the maximum allowed file size before a file rotation occurs. Set to 0 to disable rotation.
        /// </summary>
        /// <param name="maxSizeBytes">The maximum file size in bytes (minimum 4096).</param>
        public void ChangeFileSize(long maxSizeBytes)
        {
            try
            {
                var s = new Settings();
                s.MaxFileSizeBytes = maxSizeBytes;
                _maxFileSizeBytes = (long)s.MaxFileSizeBytes!;

                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely moves the active log file to a new location and resumes logging.
        /// </summary>
        /// <param name="fileLocation">The full path of the new file location.</param>
        /// <exception cref="IOException">Thrown when the file cannot be moved. The logger will be in an unusable state.</exception>
        public void ChangeFileLocation(string fileLocation)
        {
            _logChannel.Writer.Complete();
            _worker.Wait();
            _stream.Flush();
            _stream.Dispose();

            bool moved = false;
            try
            {
                File.Move(_fileLocation, fileLocation);
                _fileLocation = fileLocation;
                SaveSettings();
                moved = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_error}] Log Error: {ex.Message}");
            }
            if (moved)
            {
                _stream = new FileStream(_fileLocation, FileMode.Append, FileAccess.Write, FileShare.Read);
                _logChannel = Channel.CreateUnbounded<string>();
                _worker = Task.Run(ProcessLogsAsync);
            }
            else
            {
                throw new IOException($"Log file could not be moved to '{fileLocation}'.");
            }
        }

        private string AddSplitter(Splitter s)
        {
            return s switch
            {
                Splitter.Space => " ",
                Splitter.Asterisk => " * ",
                Splitter.Pipe => " | ",
                Splitter.Dash => " - ",
                Splitter.Colon => " : ",
                Splitter.Tab => "\t",
                Splitter.Arrow => " > ",
                Splitter.Tilde => " ~ ",
                _ => " - ",
            };
        }

        private string AddDate(DateFormat d)
        {
            return d switch
            {
                DateFormat.DateTimeWithMilliseconds => DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff"),
                DateFormat.StandardDateTime => DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                DateFormat.ShortDate => DateTime.Now.ToString("dd/MM/yyyy"),
                DateFormat.Iso8601Date => DateTime.Now.ToString("yyyy-MM-dd"),
                DateFormat.Iso8601DateTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                DateFormat.TimeOnly => DateTime.Now.ToString("HH:mm:ss"),
                DateFormat.TimeOnlyWithMilliseconds => DateTime.Now.ToString("HH:mm:ss:fff"),
                _ => "dd/MM/yyyy HH:mm:ss:fff",
            };
        }
    }
}