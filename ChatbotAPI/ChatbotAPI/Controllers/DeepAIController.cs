//using Microsoft.AspNetCore.Mvc;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;
//using Syncfusion.DocIO;
//using Syncfusion.DocIO.DLS;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using FuzzySharp;
//using ChatbotAPI.Models; // Make sure you have installed FuzzySharp NuGet package

//namespace ChatbotAPI.Controllers
//{
//    [Route("api/vector-chatbot")]
//    [ApiController]
//    public class ChatbotVectorController : ControllerBase
//    {
//        private string _documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
//        private static List<string> _vocabulary = new List<string>();
//        public static List<ImageWithText> _imageData = new List<ImageWithText>();

//        public ChatbotVectorController()
//        {
//            ExtractImagesFromDocument(_documentPath);
//        }

//        [HttpGet("get-questions")]
//        public IActionResult GetQuestions()
//        {
//            string documentText = ExtractTextFromDocx(_documentPath);
//            var questionGenerator = new QuestionGenerator();
//            List<Question> questions = questionGenerator.GenerateQuestionsFromText(documentText);

//            return Ok(questions);
//        }

//        [HttpPost("ask-vector-question")]
//        public IActionResult AskVectorQuestion([FromBody] UserQuery query)
//        {
//            if (query == null || string.IsNullOrWhiteSpace(query.Question))
//            {
//                return BadRequest("Please provide a valid question.");
//            }

//            string documentText = ExtractTextFromDocx(_documentPath);
//            var questions = GetGeneratedQuestions(documentText);



//            var result = FindAnswerAndContextWithSuggestions(documentText, query.Question, questions);

//            if (result.Answer == "Sorry, I couldn't find a relevant answer to your question.")
//            {
//                var fuzzyMatchResult = FindFuzzyMatchInDocument(documentText, query.Question);
//                if (fuzzyMatchResult != null)
//                {
//                    return Ok(new
//                    {
//                        answer = fuzzyMatchResult.Item1,
//                        context = fuzzyMatchResult.Item2,
//                        relatedQuestions = new List<string>(),
//                        images = (object)null // Explicitly setting null to avoid assignment error
//                    });
//                }
//            }

//            // Check if the question matches for image search
//            const int similarityThreshold = 60;
//            var imageResults = _imageData
//                .Where(data => Fuzz.PartialRatio(data.NearbyText, query.Question) >= similarityThreshold)
//                .Select(data => new { data.Base64Image, data.NearbyText })
//                .ToList();

//            if (imageResults.Any())
//            {
//                return Ok(new
//                {
//                    answer = result.Answer,
//                    context = result.Context,
//                    relatedQuestions = result.RelatedQuestions,
//                    images = imageResults
//                });
//            }

//            return Ok(new
//            {
//                answer = result.Answer,
//                context = result.Context,
//                relatedQuestions = result.RelatedQuestions,
//                images = (object)null // Explicitly setting null to avoid assignment error
//            });
//        }



//        [HttpPost("search-images")]
//        public IActionResult SearchImages([FromBody] string searchText)
//        {
//            if (string.IsNullOrWhiteSpace(searchText))
//            {
//                return BadRequest(new { message = "Search text cannot be empty." });
//            }

//            const int similarityThreshold = 60;

//            var results = _imageData
//                .Where(data =>
//                    Fuzz.PartialRatio(data.NearbyText, searchText) >= similarityThreshold)
//                .Select(data => new { data.Base64Image, data.NearbyText })
//                .ToList();

//            if (!results.Any())
//            {
//                var keywords = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//                results = _imageData
//                    .Where(data =>
//                        keywords.Any(keyword =>
//                            Fuzz.PartialRatio(data.NearbyText, keyword) >= similarityThreshold))
//                    .Select(data => new { data.Base64Image, data.NearbyText })
//                    .ToList();
//            }

//            if (!results.Any())
//            {
//                return Ok(new { message = "No images found for the given text." });
//            }

//            return Ok(new { images = results });
//        }

//        private string ExtractTextFromDocx(string documentPath)
//        {
//            StringBuilder documentText = new StringBuilder();

//            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(documentPath, false))
//            {
//                foreach (var paragraph in wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>())
//                {
//                    foreach (var run in paragraph.Elements<Run>())
//                    {
//                        var text = run.Elements<Text>().FirstOrDefault()?.Text;
//                        if (text != null)
//                        {
//                            documentText.Append(text + " ");
//                        }
//                    }
//                }
//            }

//            return documentText.ToString();
//        }

//        private Question FindBestVectorMatch(List<Question> questions, string query)
//        {
//            var queryVector = GenerateOneHotVector(query);

//            double bestScore = 0;
//            Question bestMatch = null;

//            foreach (var question in questions)
//            {
//                var questionVector = GenerateOneHotVector(question.Text);
//                double similarity = ComputeCosineSimilarity(queryVector, questionVector);

//                if (similarity > bestScore)
//                {
//                    bestScore = similarity;
//                    bestMatch = question;
//                }
//            }

//            return bestScore > 0.7 ? bestMatch : null;
//        }

//        private (string Answer, string Context, List<string> RelatedQuestions) FindAnswerAndContextWithSuggestions(
//            string documentText,
//            string question,
//            List<Question> questions)
//        {
//            string lowerCaseDocument = documentText.ToLower();
//            string lowerCaseQuestion = question.ToLower();

//            var relevantSentences = ExtractRelevantSentences(lowerCaseDocument, lowerCaseQuestion);

//            if (relevantSentences.Any())
//            {
//                string answer = GenerateFocusedAnswer(relevantSentences, question);
//                string context = string.Join("\n", relevantSentences);

//                return (Answer: answer, Context: context, RelatedQuestions: new List<string>());
//            }
//            else
//            {
//                var relatedQuestions = FindRelatedQuestions(questions, question);

//                return (Answer: "Sorry, I couldn't find a relevant answer to your question.", Context: "", RelatedQuestions: relatedQuestions);
//            }
//        }

//        private List<Question> GetGeneratedQuestions(string documentText)
//        {
//            var questionGenerator = new QuestionGenerator();
//            return questionGenerator.GenerateQuestionsFromText(documentText);
//        }

//        private List<string> FindRelatedQuestions(List<Question> questions, string query)
//        {
//            var relatedQuestions = new List<string>();

//            foreach (var question in questions)
//            {
//                int similarity = FuzzySharp.Fuzz.PartialRatio(question.Text, query);

//                if (similarity > 60)
//                {
//                    relatedQuestions.Add(question.Text);
//                }
//            }

//            return relatedQuestions.Take(5).ToList();
//        }

//        private List<string> ExtractRelevantSentences(string documentText, string question)
//        {
//            var relevantSentences = new List<string>();

//            var sentences = documentText.Split(new[] { '.', '!', '?', '-' }, StringSplitOptions.RemoveEmptyEntries);

//            foreach (var sentence in sentences)
//            {
//                if (ContainsQuestionPhrase(sentence, question))
//                {
//                    relevantSentences.Add(sentence.Trim());
//                }
//            }

//            return relevantSentences;
//        }

//        private bool ContainsQuestionPhrase(string sentence, string question)
//        {
//            string normalizedSentence = sentence.ToLower();
//            string normalizedQuestion = question.ToLower();

//            return normalizedSentence.Contains(normalizedQuestion) ||
//                   HasCommonKeywords(normalizedSentence, normalizedQuestion);
//        }

//        private bool HasCommonKeywords(string sentence, string question)
//        {
//            var sentenceWords = sentence.Split(' ');
//            var questionWords = question.Split(' ');

//            return sentenceWords.Intersect(questionWords).Count() > 1;
//        }

//        private string GenerateFocusedAnswer(List<string> relevantSentences, string question)
//        {
//            var sb = new StringBuilder();

//            sb.AppendLine($"Here’s what I found for your question: '{question}'");

//            foreach (var sentence in relevantSentences)
//            {
//                sb.AppendLine("- " + sentence);
//            }

//            return sb.ToString();
//        }

//        private void ExtractImagesFromDocument(string documentPath)
//        {
//            _imageData.Clear();

//            using (FileStream fileStream = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
//            using (WordDocument document = new WordDocument(fileStream, FormatType.Docx))
//            {
//                foreach (WSection section in document.Sections)
//                {
//                    foreach (WParagraph paragraph in section.Body.ChildEntities.OfType<WParagraph>())
//                    {
//                        string nearbyText = paragraph.Text.Trim();

//                        foreach (WPicture picture in paragraph.ChildEntities.OfType<WPicture>())
//                        {
//                            string base64Image = Convert.ToBase64String(picture.ImageBytes);
//                            _imageData.Add(new ImageWithText
//                            {
//                                Base64Image = base64Image,
//                                NearbyText = nearbyText
//                            });
//                        }
//                    }
//                }
//            }
//        }

//        private Tuple<string, string> FindFuzzyMatchInDocument(string documentText, string query)
//        {
//            var sentences = documentText.Split(new[] { '.', '!', '?', '-' }, StringSplitOptions.RemoveEmptyEntries);
//            string bestMatchSentence = null;
//            int bestMatchScore = 0;

//            query = query.ToLower();

//            foreach (var sentence in sentences)
//            {
//                string normalizedSentence = sentence.Trim().ToLower();

//                int similarityScore = FuzzySharp.Fuzz.PartialRatio(normalizedSentence, query);

//                if (similarityScore > bestMatchScore)
//                {
//                    bestMatchScore = similarityScore;
//                    bestMatchSentence = sentence.Trim();
//                }

//                var sentenceWords = normalizedSentence.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);
//                var queryWords = query.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

//                foreach (var word in queryWords)
//                {
//                    if (sentenceWords.Contains(word) && similarityScore > bestMatchScore)
//                    {
//                        bestMatchScore = similarityScore;
//                        bestMatchSentence = sentence.Trim();
//                    }
//                }
//            }

//            if (bestMatchScore >= 50)
//            {
//                return new Tuple<string, string>(bestMatchSentence, bestMatchSentence);
//            }

//            return null;
//        }

//        private static int[] GenerateOneHotVector(string text)
//        {
//            const int vectorSize = 10000; // Adjust the vector size based on the vocabulary size
//            int[] vector = new int[vectorSize];

//            foreach (char c in text)
//            {
//                if (c < vectorSize)
//                {
//                    vector[(int)c]++;
//                }
//            }

//            return vector;
//        }

//        private static double ComputeCosineSimilarity(int[] vector1, int[] vector2)
//        {
//            if (vector1.Length != vector2.Length)
//                throw new ArgumentException("Vectors must be of the same length.");

//            int dotProduct = 0;
//            int magnitude1 = 0;
//            int magnitude2 = 0;

//            for (int i = 0; i < vector1.Length; i++)
//            {
//                dotProduct += vector1[i] * vector2[i];
//                magnitude1 += vector1[i] * vector1[i];
//                magnitude2 += vector2[i] * vector2[i];
//            }

//            if (magnitude1 == 0 || magnitude2 == 0)
//                return 0;

//            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
//        }
//    }


//}