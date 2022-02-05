//
//  WindowsEventLog.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PortalGatewayUserRolesServer.Utility
{
    public static class WindowsEventLog
    {
        private const string eventSourceName = "Portal Gateway User Roles Server";

        public static void WriteEntry(string source, string message)
        {
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, eventSourceName);
            }

            var eventMessage = new StringBuilder();
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Source method (including namespace and class): {0}{1}", source, Environment.NewLine + Environment.NewLine);
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Message: {0}{1}", message, Environment.NewLine);

            EventLog.WriteEntry(eventSourceName, eventMessage.ToString(), EventLogEntryType.Error);
        }

        public static void WriteEntry(string source, string message, Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, eventSourceName);
            }

            var eventMessage = new StringBuilder();
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Source method (including namespace and class): {0}{1}", source, Environment.NewLine + Environment.NewLine);
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Message: {0}{1}", message, Environment.NewLine + Environment.NewLine);
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Exception: {0}{1}", ex.Message, Environment.NewLine + Environment.NewLine);
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Exception stack trace: {0}{1}{2}", Environment.NewLine, ex.StackTrace, Environment.NewLine);

            EventLog.WriteEntry(eventSourceName, eventMessage.ToString(), EventLogEntryType.Error);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType eventLogEntryType)
        {
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, eventSourceName);
            }

            var eventMessage = new StringBuilder();
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Source method (including namespace and class): {0}{1}", source, Environment.NewLine + Environment.NewLine);
            eventMessage = eventMessage.AppendFormat(CultureInfo.InvariantCulture, "Message: {0}{1}", message, Environment.NewLine);

            EventLog.WriteEntry(eventSourceName, eventMessage.ToString(), eventLogEntryType);
        }
    }
}