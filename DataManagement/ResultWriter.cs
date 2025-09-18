using System;
using System.IO;
using System.Text.Json;
using NUnit.Framework; // pour TestContext

namespace CampusFrance.Test.DataManagement
{
    public static class ResultWriter
    {
        // Résout le chemin vers Data/resultatsTests.json
        // - En CI : DATA_DIR est défini (Jenkinsfile) → ${WORKSPACE}/Data
        // - En local : retombe sur ../../../Data par rapport au dossier des tests
        private static string ResolvePath(string fileName)
        {
            var dataDirEnv = Environment.GetEnvironmentVariable("DATA_DIR");
            string dataDir;
            if (!string.IsNullOrWhiteSpace(dataDirEnv))
            {
                dataDir = dataDirEnv; // CI
            }
            else
            {
                // local : même logique que ton code actuel
                dataDir = Path.Combine("..", "..", "..", "Data");
            }

            return Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, dataDir, fileName));
        }

        public static void Clear(string fileName)
        {
            var path = ResolvePath(fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, string.Empty);
        }

        public static void Append(object obj, string fileName = "resultatsTests.json")
        {
            var path = ResolvePath(fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var line = JsonSerializer.Serialize(obj);
            File.AppendAllText(path, line + Environment.NewLine);
        }
    }
}
