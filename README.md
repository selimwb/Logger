[Türkçe](README.tr.md) | [English](README.md)
---
# **Logger**

A high-performance, asynchronous, and thread-safe file logging library built for modern .NET 10 applications.

## **Features**

* **100% Asynchronous Architecture:** Uses System.Threading.Channels in the background, ensuring the main thread is never blocked by I/O operations.  
* **Thread-Safe:** Utilizes the Singleton design pattern to work safely in multi-threaded environments without race conditions.  
* **Automatic File Rotation:** Archives the active log file and continues with a new one once the specified maximum file size is reached.  
* **Dynamic Flush Control:** Ability to customize the disk write (flush) interval in seconds.  
* **Rich Format Support:** 7 different date formats (including ISO 8601\) and 8 different separator (Splitter) options for log messages.  
* **Persistent Settings:** Configurations are automatically saved to disk (logSettings.dat) in JSON format and restored upon the next application startup.

## **Installation**

You can clone the repository, build the project, and directly include the Logger.dll in your projects.

1. Download or clone the project.  
2. Run the ```dotnet build -c Release``` command in the terminal at the project directory.  
3. Copy the Logger.dll file generated inside the bin/Release/net10.0/ folder.  
4. Add this DLL to your project using the "Add Project Reference" (or "Add Reference") option.

*(Alternatively, you can download the latest compiled DLL version from the **Releases** section on the right.)*

## **Quick Start**

Starting to log is quite simple:
``` csharp
using Logger;

// Get the Logger instance (Singleton)  
var log = Log.GetLogger();

// Standard log entry  
await log.AddLog("Application started successfully.");

// Warning and Error logs  
await log.AddWarningLog("Memory usage exceeded 80%.");  
await log.AddErrorLog("Database connection timed out.");
```

## **Customization and Settings**

You can change the log formats and behaviors on the fly while the library is running:
``` csharp
// Separate the date and message with an arrow ( > )  
log.ChangeSplitter(Splitter.Arrow);

// Set the date format to ISO 8601 (e.g., 2026-04-20 20:45:04)  
log.ChangeDateFormat(DateFormat.Iso8601DateTime);

// Archive the file when it reaches 10 MB (10485760 bytes)  
log.ChangeFileSize(10485760);

// Write (flush) logs to disk every 5 seconds  
log.ChangeFlushTime(5);

// Customize the Error and Warning prefixes  
log.ChangeErrorText("FATAL_ERROR");  
log.ChangeWarningText("WARN");
```
**Developer:** Selim Aksakallı