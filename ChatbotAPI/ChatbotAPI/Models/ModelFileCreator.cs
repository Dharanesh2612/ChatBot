namespace ChatbotAPI.Models
{
    public class ModelFileCreator
    {
        public void CreateBinaryFile()
        {
            string modelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Models"); // Adjusted to "Models" directory
            Directory.CreateDirectory(modelsDirectory); // Ensure the directory exists

            string filePath = Path.Combine(modelsDirectory, "distilbert-base-uncased.onnx");

            if (!File.Exists(filePath))
            {
                // Replace this with the actual byte array of your model
                byte[] dummyData = new byte[] { 0x01, 0x02, 0x03 };
                File.WriteAllBytes(filePath, dummyData);
                Console.WriteLine($"Binary file created at: {filePath}");
            }
            else
            {
                Console.WriteLine("Binary file already exists.");
            }
        }
    }
}
