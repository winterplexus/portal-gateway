//
//  HttpResponseFilterStream.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using PortalGatewayModule.Log;
using PortalGatewayModule.Utility;

namespace PortalGatewayModule
{
    public class HttpResponseFilterStream : MemoryStream
    {
        private readonly Stream outputStream;
        private bool isClosing;
        private bool isClosed;

        public HttpResponseFilterStream(Stream outputStream)
        {
            this.outputStream = outputStream;
        }

        public override void Flush()
        {
            if (!isClosing || isClosed)
            {
                return;
            }

            var encoding = HttpContext.Current.Response.ContentEncoding;
            var html = encoding.GetString(ToArray());

            var contentControlFilePath = Assistant.GetConfigurationValue("ContentControlFilePath");
            if (!string.IsNullOrEmpty(contentControlFilePath))
            {
                html = ContentControl(html, contentControlFilePath);
            }

            var buffer = encoding.GetBytes(html);
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Flush();
        }

        public override void Close()
        {
            isClosing = true;      
            Flush();               
            isClosing = false;
            isClosed = true; 
            outputStream.Close();
        }

        private static string ContentControl(string html, string contentControlFilePath)
        {
            try
            {
                var recordNumber = 0;

                using (var stream = new FileStream(contentControlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var fileReader = new StreamReader(stream);

                    string replacementParameters;
                    while ((replacementParameters = fileReader.ReadLine()) != null)
                    {
                        recordNumber++;
                        html = ModifyContent(html, replacementParameters, recordNumber);
                    }
                }
            }
            catch (IOException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "I/O exception while reading content control file {0}", contentControlFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);

                EventsLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }

            return html;
        }

        private static string ModifyContent(string html, string replacementParameters, int recordNumber)
        {
            try
            {
                var enumerator = new CommaSeparatedValues().Parse(replacementParameters);

                var findText = string.Empty;
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    findText = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }

                var replaceText = string.Empty;
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    replaceText = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }

                html = html.Replace(findText, replaceText);
            }
            catch (ArgumentNullException ane)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "argument null exception while processing replacement parameters (line number {0}): {1}", recordNumber, replacementParameters);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ane);

                EventsLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ane);
            }
            catch (InvalidOperationException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "invalid operation exception while processing replacement parameters (line number {0}): {1}", recordNumber, replacementParameters);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);

                EventsLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }

            return html;
        }
    }
}