namespace ChatbotAPI.Models
{
    public class ImageWithText
    {
        public string Base64Image { get; set; } // Base64-encoded image
        public string NearbyText { get; set; }  // Text near the image

        public string Hyperlink { get; set; }
    }
}


