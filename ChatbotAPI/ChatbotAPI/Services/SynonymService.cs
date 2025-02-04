using Newtonsoft.Json;

namespace ChatbotAPI.Services
{
   
public class SynonymService
    {
        // Dictionary to hold the synonyms data
        private static Dictionary<string, List<string>> _synonyms;

        // Static constructor to load the synonyms file when the service is initialized
        static SynonymService()
        {
            // Load synonyms from JSON file when the application starts
            LoadSynonymsFromJson("C:\\Users\\dharanesh.r\\Documents\\synonyms.json"); // Replace this with your file path
        }

        // Public property to access the loaded synonyms
        public static Dictionary<string, List<string>> Synonyms => _synonyms;

        // Method to load synonyms from a JSON file
        private static void LoadSynonymsFromJson(string filePath)
        {
            try
            {
                // Read the JSON file contents
                var json = File.ReadAllText(filePath);

                // Deserialize JSON into a Dictionary<string, List<string>>
                _synonyms = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., file not found, invalid JSON, etc.)
                Console.WriteLine($"Error loading synonyms from file: {ex.Message}");
            }
        }

    }
}
