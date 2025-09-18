using System;
using System.IO;
using System.Text.Json;

namespace CampusFrance.Test.DataManagement
{
    public static class JsonLoader
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static T[] LoadArray<T>(string path)
        {
            // Si DATA_DIR est défini (CI), on remappe les chemins relatifs "Data/xxx.json"
            var dataDir = Environment.GetEnvironmentVariable("DATA_DIR");
            if (!string.IsNullOrWhiteSpace(dataDir) && !Path.IsPathRooted(path))
            {
                // normalise pour attraper "Data/...":
                var norm = path.Replace('\\', '/');
                if (norm.StartsWith("Data/", StringComparison.OrdinalIgnoreCase))
                    norm = norm.Substring("Data/".Length);

                // reconstruit dans ${DATA_DIR}
                path = Path.Combine(dataDir, norm);
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T[]>(json, Options) ?? [];
        }
    }
}
