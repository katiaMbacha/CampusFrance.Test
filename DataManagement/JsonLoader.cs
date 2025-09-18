using System.IO;
using System.Text.Json;

namespace CampusFrance.Test.DataManagement
{
    public static class JsonLoader
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,   // insensible à la casse
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true            // permet les virgules finales
        };

        /// <summary>
        /// Charge un tableau d’objets T à partir d’un fichier JSON.
        /// Exemple : var data = JsonLoader.LoadArray<StudentData>("Data/students.json");
        /// </summary>
        public static T[] LoadArray<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T[]>(json, Options) ?? [];
        }
    }
}
