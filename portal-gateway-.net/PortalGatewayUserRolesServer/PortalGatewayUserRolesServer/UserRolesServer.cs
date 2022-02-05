//
//  UserRolesServer.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using PortalGatewayUserRolesServer.Utility;

namespace PortalGatewayUserRolesServer
{
    public class UserRolesServer
    {
        private string userRolesFilePath;
        private string userRolesDataFilePath;

        public UserRolesServer()
        {
            GetUserRolesFilePath();
            GetUserRolesDataFilePath();
        }

        public string Search(string userId)
        {
            if (userRolesFilePath == null || userRolesDataFilePath == null)
            {
                return "system error";
            }
            if (string.IsNullOrEmpty(userId))
            {
                return "user ID is missing or empty";
            }

            CopyUserRolesFile();

            var searchUserId = userId.ToUpper(CultureInfo.InvariantCulture);

            return GetRoles(searchUserId);
        }

        public int Count()
        {
            if (userRolesFilePath == null || userRolesDataFilePath == null)
            {
                return 0;
            }

            CopyUserRolesFile();

            return GetRolesCount();
        }

        private void CopyUserRolesFile()
        {
            var temporaryUserRolesDataFilePath = userRolesDataFilePath + ".tmp";

            try
            {
                if (!File.Exists(userRolesFilePath))
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "User roles file is missing: {0}", userRolesFilePath);

                    WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, EventLogEntryType.Information);

                    return;
                }

                var userRolesFileLastWriteTime = File.GetLastWriteTime(userRolesFilePath);

                if (!File.Exists(userRolesDataFilePath))
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "User roles data file is missing: {0}", userRolesFilePath);

                    WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, EventLogEntryType.Information);

                    File.Copy(userRolesFilePath, temporaryUserRolesDataFilePath, true);
                    if (File.Exists(temporaryUserRolesDataFilePath))
                    {
                        File.Copy(temporaryUserRolesDataFilePath, userRolesDataFilePath, true);
                    }
                    return;
                }

                var userRolesDataFileLastWriteTime = File.GetLastWriteTime(userRolesDataFilePath);

                if (userRolesFileLastWriteTime.CompareTo(userRolesDataFileLastWriteTime) <= 0)
                {
                    return;
                }

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "User roles file has changed", EventLogEntryType.Information);

                File.Copy(userRolesFilePath, temporaryUserRolesDataFilePath, true);
                if (File.Exists(temporaryUserRolesDataFilePath))
                {
                    File.Copy(temporaryUserRolesDataFilePath, userRolesDataFilePath, true);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Unauthorized access exception for either user roles file {0} or user roles data file {1}", userRolesFilePath, userRolesDataFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, uae);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Directory not found exception for either user roles file {0} or user roles data file {1}", userRolesFilePath, userRolesDataFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, dnfe);
            }
            catch (FileNotFoundException fnfe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "File not found exception using user roles file {0}", userRolesFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, fnfe);
            }
            catch (IOException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "I/O exception for either user roles file {0} or user roles data file {1}", userRolesFilePath, userRolesDataFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }
        }

        private string GetRoles(string userId)
        {
            try
            {
                var recordNumber = 0;

                using (var stream = new FileStream(userRolesDataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var fileReader = new StreamReader(stream);

                    string record;
                    while ((record = fileReader.ReadLine()) != null)
                    {
                        recordNumber++;

                        var userRoles = ParseRecord(record, recordNumber);
                        if (userRoles == null)
                        {
                            continue;
                        }

                        if (userId == userRoles.UserId)
                        {
                            return userRoles.Roles;
                        }
                    }
                }
            }
            catch (IOException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "I/O exception using user roles data file {0}", userRolesDataFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }

            return null;
        }

        private int GetRolesCount()
        {
            try
            {
                var recordNumber = 0;

                using (var stream = new FileStream(userRolesDataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var fileReader = new StreamReader(stream);

                    string record;
                    while ((record = fileReader.ReadLine()) != null)
                    {
                        recordNumber++;

                        var userRoles = ParseRecord(record, recordNumber);
                        if (userRoles == null)
                        {
                            recordNumber--;
                        }
                    }
                }

                return recordNumber;
            }
            catch (IOException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "I/O exception using user roles data file {0}", userRolesDataFilePath);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }

            return 0;
        }

        private static UserRoles ParseRecord(string record, int recordNumber)
        {
            var userRoles = new UserRoles();

            try
            {
                var enumerator = new CommaSeparatedValues().Parse(record);

                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.RegionCode = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.UserId = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.Roles = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.City = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.FullName = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
                if (enumerator.MoveNext() || enumerator.Current != null)
                {
                    userRoles.SupervisorsFullName = Convert.ToString(enumerator.Current, CultureInfo.InvariantCulture).ToUpperInvariant().Trim();
                }
            }
            catch (ArgumentNullException ane)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Argument null exception while processing user roles data file record {0}: {1}", recordNumber, record);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ane);
            }
            catch (InvalidOperationException ioe)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Invalid operation exception while processing user roles data file record {0}: {1}", recordNumber, record);

                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, ioe);
            }
            return userRoles;
        }

        private void GetUserRolesFilePath()
        {
            userRolesFilePath = Assistant.GetConfigurationValue("UserRolesFilePath");
            if (string.IsNullOrEmpty(userRolesFilePath))
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "Invalid user roles file path configuration value");
            }
        }

        private void GetUserRolesDataFilePath()
        {
            userRolesDataFilePath = Assistant.GetConfigurationValue("UserRolesDataFilePath");
            if (string.IsNullOrEmpty(userRolesDataFilePath))
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "Invalid user roles data file path configuration value");
            }
        }
    }
}