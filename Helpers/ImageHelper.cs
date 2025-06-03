using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Helper class for managing exercise images
    /// </summary>
    public static class ImageHelper
    {
        private static readonly string ILLUSTRATIONS_FOLDER = "Illustrations";
        private static readonly string ILLUSTRATIONS_PATH;

        static ImageHelper()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ILLUSTRATIONS_PATH = Path.Combine(baseDirectory, ILLUSTRATIONS_FOLDER);
        }

        /// <summary>
        /// Get the full path to the illustrations directory
        /// </summary>
        /// <returns>Full path to illustrations directory</returns>
        public static string GetIllustrationsDirectory()
        {
            return ILLUSTRATIONS_PATH;
        }

        /// <summary>
        /// Check if the illustrations directory exists
        /// </summary>
        /// <returns>True if directory exists</returns>
        public static bool IllustrationsDirectoryExists()
        {
            return Directory.Exists(ILLUSTRATIONS_PATH);
        }

        /// <summary>
        /// Create the illustrations directory if it doesn't exist
        /// </summary>
        /// <returns>True if directory exists or was created successfully</returns>
        public static bool EnsureIllustrationsDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(ILLUSTRATIONS_PATH))
                {
                    Directory.CreateDirectory(ILLUSTRATIONS_PATH);
                    System.Diagnostics.Debug.WriteLine($"Created illustrations directory: {ILLUSTRATIONS_PATH}");
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create illustrations directory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate if an image filename is valid
        /// </summary>
        /// <param name="imageFileName">The image filename to validate</param>
        /// <returns>True if filename is valid (ends with .png and has no invalid characters)</returns>
        public static bool IsValidImageFileName(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
                return false;

            // Check file extension
            if (!imageFileName.ToLowerInvariant().EndsWith(".png"))
                return false;

            // Check for invalid filename characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (imageFileName.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if an exercise image file exists
        /// </summary>
        /// <param name="imageFileName">The image filename</param>
        /// <returns>True if file exists</returns>
        public static bool ExerciseImageExists(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
                return false;

            string fullPath = Path.Combine(ILLUSTRATIONS_PATH, imageFileName);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Get the full path to an exercise image
        /// </summary>
        /// <param name="imageFileName">The image filename</param>
        /// <returns>Full path to the image file</returns>
        public static string GetImagePath(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
                return null;

            return Path.Combine(ILLUSTRATIONS_PATH, imageFileName);
        }

        /// <summary>
        /// Load a BitmapImage from an exercise image file
        /// </summary>
        /// <param name="imageFileName">The image filename</param>
        /// <returns>BitmapImage if successful, null otherwise</returns>
        public static BitmapImage LoadExerciseImage(string imageFileName)
        {
            if (!IsValidImageFileName(imageFileName))
                return null;

            string imagePath = GetImagePath(imageFileName);

            if (!File.Exists(imagePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load immediately and release file handle
                bitmap.EndInit();
                bitmap.Freeze(); // Make it thread-safe

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image '{imageFileName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get information about available exercise images
        /// </summary>
        /// <returns>Array of image filenames in the illustrations directory</returns>
        public static string[] GetAvailableImages()
        {
            try
            {
                if (!Directory.Exists(ILLUSTRATIONS_PATH))
                    return new string[0];

                return Directory.GetFiles(ILLUSTRATIONS_PATH, "*.png", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileName)
                    .ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting available images: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// Get a status report of the illustrations system
        /// </summary>
        /// <returns>Status string for display in the application</returns>
        public static string GetImageSystemStatus()
        {
            if (!IllustrationsDirectoryExists())
            {
                return "❌ Illustrations folder not found";
            }

            var availableImages = GetAvailableImages();

            if (availableImages.Length == 0)
            {
                return $"⚠️ Illustrations folder empty\nPath: {ILLUSTRATIONS_PATH}";
            }

            return $"✅ Found {availableImages.Length} exercise images\nPath: {ILLUSTRATIONS_PATH}";
        }
    }
}