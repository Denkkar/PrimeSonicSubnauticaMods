﻿namespace Common
{
    using System;
    using System.Reflection;

    internal static class QuickLogger
    {
        internal static bool DebugLogsEnabled = false;

        public static void Info(string msg, bool showOnScreen = false, Assembly callingAssembly = null)
        {
            string name = (callingAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;

            Console.WriteLine($"[{name}:INFO] {msg}");

            if (showOnScreen)
                ErrorMessage.AddMessage(msg);
        }

        public static void Debug(string msg, bool showOnScreen = false, Assembly callingAssembly = null)
        {
            if (!DebugLogsEnabled)
                return;

            string name = (callingAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;

            Console.WriteLine($"[{name}:DEBUG] {msg}");

            if (showOnScreen)
                ErrorMessage.AddDebug(msg);
        }

        public static void Error(string msg, bool showOnScreen = false, Assembly callingAssembly = null)
        {
            string name = (callingAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {msg}");

            if (showOnScreen)
                ErrorMessage.AddError(msg);
        }

        public static void Error(string msg, Exception ex, Assembly callingAssembly = null)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {msg}{Environment.NewLine}{ex.ToString()}");
        }

        public static void Error(Exception ex, Assembly callingAssembly = null)
        {
            string name = (callingAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;

            Console.WriteLine($"[{name}:ERROR] {ex.ToString()}");
        }

        public static void Warning(string msg, bool showOnScreen = false, Assembly callingAssembly = null)
        {
            string name = (callingAssembly ?? Assembly.GetCallingAssembly()).GetName().Name;

            Console.WriteLine($"[{name}:WARN] {msg}");

            if (showOnScreen)
                ErrorMessage.AddWarning(msg);
        }

        public static string GetAssemblyVersion()
        {
            return GetAssemblyVersion(Assembly.GetExecutingAssembly());
        }

        public static string GetAssemblyVersion(Assembly assembly)
        {
            Version version = assembly.GetName().Version;

            //      Major Version
            //      Minor Version
            //      Build Number
            //      Revision

            if (version.Revision > 0)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }

            if (version.Build > 0)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }

            if (version.Minor > 0)
            {
                return $"{version.Major}.{version.Minor}";
            }

            return $"{version.Major}";
        }
    }
}
