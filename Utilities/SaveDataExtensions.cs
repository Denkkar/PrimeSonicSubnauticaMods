﻿namespace Common
{
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Common.EasyMarkup;

    internal static class SaveDataExtensions
    {
        public static string DefaultFileLocation<T>(this T data, string executingLocation = null) where T : EmProperty
        {
            executingLocation = executingLocation ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(executingLocation, $"{data.Key}.txt");
        }

        public static void Save<T>(this T data, string directory, string fileLocation, string extraText = null) where T : EmProperty
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fileLocation, (extraText ?? string.Empty) + data.PrettyPrint(), Encoding.UTF8);
        }

        public static void Save<T>(this T data, string extraText = null) where T : EmProperty
        {
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            data.Save(executingLocation, data.DefaultFileLocation(executingLocation), extraText);
        }

        public static bool Load<T>(this T data, string directory, string fileLocation) where T : EmProperty
        {
            if (!File.Exists(fileLocation))
            {
                data.Save(directory, fileLocation);
                return false;
            }

            string serializedData = File.ReadAllText(fileLocation);

            bool validData = data.FromString(serializedData);

            if (!validData)
            {
                data.Save(directory, fileLocation);
                return false;
            }

            return true;
        }

        public static bool Load<T>(this T data) where T : EmProperty
        {
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return data.Load(executingLocation, Path.Combine(executingLocation, $"{data.Key}.txt"));
        }

        public static bool HasData<T>(this T property) where T : EmProperty
        {
            if (!property.HasValue)
            {
                QuickLogger.Warning($"Value for '{property.Key}' was invalid or out of range.");
                return false;
            }

            return true;
        }

        public static bool HasDataInRange<T>(this T property, int minValue, int maxValue) where T : EmProperty<int>
        {
            if (!property.HasValue || property.Value < minValue || property.Value > maxValue)
            {
                QuickLogger.Warning($"Value for '{property.Key}' was invalid or out of range.");
                return false;
            }

            return true;
        }

        public static bool HasDataInRange<T>(this T property, float minValue, float maxValue) where T : EmProperty<float>
        {
            if (!property.HasValue || property.Value < minValue || property.Value > maxValue)
            {
                QuickLogger.Warning($"Value for '{property.Key}' was invalid or out of range.");
                return false;
            }

            return true;
        }
    }
}
