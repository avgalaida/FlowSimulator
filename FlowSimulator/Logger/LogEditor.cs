using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using FlowGraphBase.Logger;

namespace FlowSimulator.Logger
{
    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }

    public class LogEditor
        : ILog
    {
        public static ObservableCollection<LogEntry> LogEntries { get; private set; }

        public LogEditor()
        {
            if (LogEntries == null)
            {
                LogEntries = new ObservableCollection<LogEntry>();
            }
        }

        public void Close()
        {

        }

        public void Write(LogVerbosity verbose, string msg)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                LogEntries.Add(new LogEntry
                {
                    Severity = "[" + Enum.GetName(typeof(LogVerbosity), verbose) + "]",
                    DateTime = DateTime.Now,
                    Message = msg,
                });
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action( () => Write(verbose, msg)));
            }
        }
    }
}
