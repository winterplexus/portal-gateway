//
//  Email.cs
//
//  Fl. Department of Revenue 2015-17 (FPLS)
//
using System;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;

namespace PortalGateway.Utility
{
    public static class Email
    {
        public static void NotifyAdministrator(string action, Exception ex)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            try
            {
                SendExceptionByEmail(action, ex);
            }
            catch (InvalidOperationException ioe)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "invalid operation exception", ioe);
            }
            catch (SmtpFailedRecipientsException sfre)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "SMTP failed recipients exception", sfre);
            }
            catch (SmtpException se)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "SMTP exception", se);
            }
        }

        private static void SendExceptionByEmail(string action, Exception ex)
        {
            var subject = string.Format(CultureInfo.InvariantCulture, "Service request '{0}' failed", action);

            var body = string.Format(CultureInfo.InvariantCulture, "Exception: {0}{1}", ex.Message, Environment.NewLine + Environment.NewLine);

            if (ex.InnerException != null)
            {
                body += string.Format(CultureInfo.InvariantCulture, "Inner exception: {0}{1}", ex.InnerException.Message, Environment.NewLine + Environment.NewLine);
            }
            if (ex.StackTrace != null)
            {
                body += string.Format(CultureInfo.InvariantCulture, "Stack trace: {0}{1}", ex.StackTrace, Environment.NewLine + Environment.NewLine);
            }

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(Assistant.GetConfigurationValue("EmailSender"));
                message.To.Add(Assistant.GetConfigurationValue("EmailRecipient"));
                message.Subject = subject;
                message.Body = body;

                using (var client = new SmtpClient(Assistant.GetConfigurationValue("EmailServer")))
                {
                    client.Credentials = null;
                    client.Send(message);
                }
            }
        }
    }
}                    