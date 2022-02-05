//
//  EventsLog.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System;
using System.Globalization;
using NLog;
using PortalGatewayModule.Utility;

[assembly: CLSCompliant(true)]
namespace PortalGatewayModule.Log
{
    public static class EventsLog
    {
        public static void WriteEvent(string source, string message)
        {
            var logger = LogManager.GetLogger("Events");

            logger.Info(CultureInfo.InvariantCulture, "source method (including namespace and class): {0}", source);
            logger.Info(CultureInfo.InvariantCulture, "message: {0}", message);
        }

        public static void WriteEvent(string source, string message, Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            var logger = LogManager.GetLogger("Events");

            logger.Info(CultureInfo.InvariantCulture, "source method (including namespace and class): {0}", source);
            logger.Info(CultureInfo.InvariantCulture, "message: {0}", message);
            logger.Info(CultureInfo.InvariantCulture, "exception: {0}", ex.Message);
            logger.Info(CultureInfo.InvariantCulture, "exception stack trace: {0}{1}", Environment.NewLine, ReplaceControlCharacters(ex.StackTrace));
        }

        private static string ReplaceControlCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.Replace(EscapeCharacters.Bell, "").
                                                               Replace(EscapeCharacters.Backspace, "").
                                                               Replace(EscapeCharacters.FormFeed, " | ").
                                                               Replace(EscapeCharacters.Linefeed, "").
                                                               Replace(EscapeCharacters.CarriageReturn, " . ").
                                                               Replace(EscapeCharacters.HorizontalTab, " ").
                                                               Replace(EscapeCharacters.VerticalTab, " | ");
        }
    }
}