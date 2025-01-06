namespace ChatbotAPI.Models
{
    public class Question
    {
        public string Text { get; set; }   // The question text
        public string Context { get; set; } // The context that corresponds to the question (e.g., the sentence it was generated from)
    }
}
