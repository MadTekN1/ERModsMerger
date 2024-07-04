using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERModsMerger.Core
{
    public static class Utils
    {
        public static string GetInstallPath(string applicationName)
        {
            var installPath = FindApplicationPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", applicationName);

            if (installPath == null)
            {
                installPath = FindApplicationPath(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", applicationName);
            }

            return installPath;
        }

        private static string FindApplicationPath(string keyPath, string applicationName)
        {

            var hklm = Registry.LocalMachine;
            var uninstall = hklm.OpenSubKey(keyPath);
            foreach (var productSubKey in uninstall.GetSubKeyNames())
            {
                var product = uninstall.OpenSubKey(productSubKey);

                var displayName = product.GetValue("DisplayName");
                if (displayName != null && displayName.ToString() == applicationName)
                {
                    return product.GetValue("InstallLocation").ToString();
                }

            }

            return null;
        }

        /// <summary>
        ///     Returns string representing version of param or regulation.
        /// </summary>
        public static string ParseParamVersion(ulong version)
        {
            string verStr = version.ToString();
            if (verStr.Length == 7 || verStr.Length == 8)
            {
                char major = verStr[0];
                string minor = verStr[1..3];
                char patch = verStr[3];
                string rev = verStr[4..];
                return $"{major}.{minor}.{patch}";
            }

            return "Unknown version format";
        }


        /// <summary>
        /// Same as normal Equals but can also check equality between Enumerables/Arrays
        /// </summary>
        /// <returns></returns>
        public static bool AdvancedEquals(object? x, object? y)
        {
            if (x is null || y is null)
                return false;

            if (x is IEnumerable a && y is IEnumerable b)
                return a.Cast<object>().SequenceEqual(b.Cast<object>());

            return x.Equals(y);
        }
    }
}
