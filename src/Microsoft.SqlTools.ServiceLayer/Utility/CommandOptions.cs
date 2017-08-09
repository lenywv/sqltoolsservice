//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.SqlTools.ServiceLayer.Utility
{
    /// <summary>
    /// The command-line options helper class.
    /// </summary>
    internal class CommandOptions
    {
        /// <summary>
        /// Construct and parse command line options from the arguments array
        /// </summary>
        public CommandOptions(string[] args)
        {
            ErrorMessage = string.Empty;
            Locale = string.Empty;
            List<string> argListForMSSqlTools = new List<string>();

            try
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    string arg = args[i];
                    if (arg.StartsWith("--") || arg.StartsWith("-"))
                    {
                        // Extracting arguments and properties
                        arg = arg.Substring(1).ToLowerInvariant();
                        string argName = arg;

                        switch (argName)
                        {
                            case "-enable-logging":
                                EnableLogging = true;
                                argListForMSSqlTools.Add(argName);
                                break;
                            case "-log-dir":
                                SetLoggingDirectory(args[++i]);
                                argListForMSSqlTools.Add(argName);
                                argListForMSSqlTools.Add(args[++i]);
                                break;
                            case "-mssqltools-exec":
                                SetMsSqlToolsPath(args[++i]);
                                break;
                            case "-locale":
                                SetLocale(args[++i]);
                                argListForMSSqlTools.Add(argName);
                                break;
                            case "h":
                            case "-help":
                                ShouldExit = true;
                                return;
                            default:
                                ErrorMessage += String.Format("Unknown argument \"{0}\"" + Environment.NewLine, argName);
                                break;
                        }
                    }
                }
                MSSqlToolsArgs= argListForMSSqlTools.ToArray();
            }
            catch (Exception ex)
            {
                ErrorMessage += ex.ToString();
                return;
            }
            finally
            {
                if (!string.IsNullOrEmpty(ErrorMessage) || ShouldExit)
                {
                    Console.WriteLine(Usage);
                    ShouldExit = true;
                }
            }
        }

        internal string ErrorMessage { get; private set; }


        /// <summary>
        /// Whether diagnostic logging is enabled
        /// </summary>
        public bool EnableLogging { get; private set; }

        /// <summary>
        /// Gets the directory where log files are output.
        /// </summary>
        public string LoggingDirectory { get; private set; }

        /// <summary>
        /// Gets the path where the language server for ms sql is
        /// </summary>
        public string MSSqlToolsPath { get; private set; }
        
        /// <summary>
        /// Gets the arguments for mssql language server
        /// </summary>
        public string[] MSSqlToolsArgs { get;}

        /// <summary>
        /// Whether the program should exit immediately. Set to true when the usage is printed.
        /// </summary>
        public bool ShouldExit { get; private set; }

        /// <summary>
        /// The locale our we should instantiate this service in 
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Get the usage string describing command-line arguments for the program
        /// </summary>
        public string Usage
        {
            get
            {
                var str = string.Format("{0}" + Environment.NewLine +
                    "Microsoft.SqlTools.ServiceLayer.exe " + Environment.NewLine +
                    "   Options:" + Environment.NewLine +
                    "        [--enable-logging]" + Environment.NewLine +
                    "        [--log-dir **] (default: current directory)" + Environment.NewLine +
                    "        [--help]" + Environment.NewLine +
                    "        [--locale **] (default: 'en')" + Environment.NewLine,
                    ErrorMessage);
                return str;
            }
        }

        private void SetLoggingDirectory(string loggingDirectory)
        {
            if (string.IsNullOrWhiteSpace(loggingDirectory))
            {
                return;
            }

            this.LoggingDirectory = Path.GetFullPath(loggingDirectory);
        }

        private void SetMsSqlToolsPath(string msSqlToolsPath)
        {
            if (string.IsNullOrWhiteSpace(msSqlToolsPath))
            {
                return;
            }

            this.MSSqlToolsPath = msSqlToolsPath;
        }

        private void SetLocale(string locale)
        {
            try
            {
                // Creating cultureInfo from our given locale
                CultureInfo language = new CultureInfo(locale);
                Locale = locale;

                // Setting our language globally 
                CultureInfo.CurrentCulture = language;
                CultureInfo.CurrentUICulture = language;

                // Setting our internal SR culture to our global culture
                SR.Culture = CultureInfo.CurrentCulture;
            }
            catch (CultureNotFoundException)
            {
                // Ignore CultureNotFoundException since it only is thrown before Windows 10.  Windows 10,
                // along with macOS and Linux, pick up the default culture if an invalid locale is passed
                // into the CultureInfo constructor.
            }
        }
    }
}
