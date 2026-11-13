using System.IO;
using System.Text.Json;
using LABB_3.Model;

namespace LABB_3.Services
{
    public class JsonDataService
    {
        private readonly string _folderPath;
        private readonly string _filePath;

        public JsonDataService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _folderPath = Path.Combine(appData, "Labb3MathQuiz");
            _filePath = Path.Combine(_folderPath, "QuestionPacks.json");
        }

        public async Task<List<QuestionPack>> LoadAsync()
        {
            if (!File.Exists(_filePath))
                return new List<QuestionPack>();

            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<QuestionPack>>(json) ?? new List<QuestionPack>();
        }

        public async Task SaveAsync(IEnumerable<QuestionPack> packs)
        {
            Directory.CreateDirectory(_folderPath);   // does nothing if the folder already exists

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(packs, options);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}