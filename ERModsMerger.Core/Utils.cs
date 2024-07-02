using Microsoft.Win32;
using System;
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
    }
}
