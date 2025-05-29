using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows;

namespace TriviaExercise.Helpers
{
    public static class StartupHelper
    {
        private const string STARTUP_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "TriviaExercise";

        /// <summary>
        /// Registers the application to start with Windows
        /// </summary>
        /// <param name="startMinimized">If true, adds --minimized parameter to start minimized</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool RegisterStartup(bool startMinimized = true)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, true))
                {
                    if (key == null)
                        return false;

                    string executablePath = Assembly.GetExecutingAssembly().Location;

                    // For .NET Core/.NET 5+ applications, use the .exe path instead of .dll
                    if (executablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        executablePath = executablePath.Replace(".dll", ".exe");
                    }

                    // Add startup parameter if requested
                    string startupCommand = startMinimized ?
                        $"\"{executablePath}\" --minimized" :
                        $"\"{executablePath}\"";

                    key.SetValue(APP_NAME, startupCommand);
                    return true;
                }
            }
            catch (SecurityException ex)
            {
                MessageBox.Show($"Security error: Unable to register startup. {ex.Message}",
                    "Security Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: Unable to register startup. {ex.Message}",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering startup: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Unregisters the application from Windows startup
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool UnregisterStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, true))
                {
                    if (key == null)
                        return false;

                    key.DeleteValue(APP_NAME, false); // false = don't throw if key doesn't exist
                    return true;
                }
            }
            catch (SecurityException ex)
            {
                MessageBox.Show($"Security error: Unable to unregister startup. {ex.Message}",
                    "Security Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: Unable to unregister startup. {ex.Message}",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unregistering startup: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Checks if the application is registered for Windows startup
        /// </summary>
        /// <returns>True if registered, false otherwise</returns>
        public static bool IsRegisteredForStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, false))
                {
                    if (key == null)
                        return false;

                    object value = key.GetValue(APP_NAME);
                    return value != null;
                }
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current startup command registered in the registry
        /// </summary>
        /// <returns>The startup command or null if not registered</returns>
        public static string GetStartupCommand()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, false))
                {
                    if (key == null)
                        return null;

                    return key.GetValue(APP_NAME)?.ToString();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}