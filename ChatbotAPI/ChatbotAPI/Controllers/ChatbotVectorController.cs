using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FuzzySharp;
using ChatbotAPI.Models;
using System.Text.RegularExpressions;
using ChatbotAPI.Services;

namespace ChatbotAPI.Controllers
{
    [Route("api/vector-chatbot")]
    [ApiController]
    public class ChatbotVectorController : ControllerBase
    {
        private string _documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
        private static List<string> _vocabulary = new List<string>();
        public static List<ImageWithText> _imageData = new List<ImageWithText>();


        public ChatbotVectorController()
        {
            ExtractImagesFromDocument(_documentPath);
        }

        private static Dictionary<string, string> _fixedTextToImage = new Dictionary<string, string>
        {
            { "claims create,create claims,claims,what are does claims do,Can you explain claims?", "Image_1.png,Image_2.png" },
            { "manual check,check manual", "Image_6.png" },
            { "Create and Fill the Claim Form,how to create and fill the claim form", "Image_2.png" },
            {"Medical Service Information,Automatic Data Validation" ,"Image_4.png"},
            { "Billing and Insurance Information", "Image_5.png" },
            { "Post Submission and Follow Up", "Image_2.png,Image_4.png" },
            {"Required Documents,Documents required","Image_1.png" },
        };


        [HttpGet("get-questions")]
        public IActionResult GetQuestions()
        {
            string documentText = ExtractTextFromDocx(_documentPath);
            var questionGenerator = new QuestionGenerator();
            List<Question> questions = questionGenerator.GenerateQuestionsFromText(documentText);

            return Ok(questions);
        }

        [HttpPost("ask-vector-question")]
        public IActionResult AskVectorQuestion([FromBody] UserQuery query)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Question))
            {
                return BadRequest("Please provide a valid question.");
            }

            string normalizedQuery = NormalizeQuery(query.Question.ToLower());
            string documentText = ExtractTextFromDocx(_documentPath);
            var questions = GetGeneratedQuestions(documentText);

            // Get the answer and context based on doc text
            var result = FindAnswerAndContextWithSuggestions(documentText, normalizedQuery, questions);

            if (result.Answer == "Sorry, I couldn't find a relevant answer to your question.")
            {
                var fuzzyMatchResult = FindFuzzyMatchInDocument(documentText, normalizedQuery);
                if (fuzzyMatchResult != null)
                {
                    return Ok(new
                    {
                        answer = fuzzyMatchResult.Item1,
                        context = fuzzyMatchResult.Item2,
                        relatedQuestions = new List<string>(),
                        images = (object)null
                    });
                }
            }

            // Return results with images matching fixed text
            const int similarityThreshold = 60;
            var fixedTextResults = _fixedTextToImage
                .Where(entry => Fuzz.PartialRatio(entry.Key, normalizedQuery) >= similarityThreshold)
                .Select(entry =>
                {
                    var base64Images = entry.Value.Split(',')
                        .Where(imageName => !string.IsNullOrWhiteSpace(imageName))
                        .Select(imageName =>
                        {
                            var imageData = _imageData.FirstOrDefault(img => $"Image_{_imageData.IndexOf(img) + 1}.png" == imageName);
                            return imageData?.Base64Image;
                        })
                        .Where(img => !string.IsNullOrEmpty(img))
                        .ToList();

                    return new
                    {
                        answer = result.Answer + ".",
                        context = result.Context,
                        images = base64Images,
                        NearbyText = entry.Key
                    };
                })
                .ToList();

            if (fixedTextResults.Any())
            {
                return Ok(new
                {
                    answer = result.Answer + ".",
                    context = result.Context,
                    images = fixedTextResults
                });
            }

            // Check the document's image data for approximate matches
            var imageResults = _imageData
                .Where(data => Fuzz.PartialRatio(data.NearbyText, normalizedQuery) >= similarityThreshold)
                .Select(data => new { data.Base64Image, data.NearbyText })
                .ToList();

            if (!imageResults.Any())
            {
                return Ok(new
                {
                    answer = result.Answer + ".",
                    context = result.Context,
                    images = (object)null
                });
            }

            // Return the answer, context, and images if image results are found
            return Ok(new
            {
                answer = result.Answer + ".",
                context = result.Context,
                images = imageResults
            });
        }



        private Tuple<string, string> FindFuzzyMatchInDocument(string documentText, string query)
        {
            var sentences = documentText.Split(new[] { '.', '!', '?', '-' }, StringSplitOptions.RemoveEmptyEntries);
            string bestMatchSentence = null;
            int bestMatchScore = 0;

            query = query.ToLower();

            foreach (var sentence in sentences)
            {
                string normalizedSentence = sentence.Trim().ToLower();

                int similarityScore = FuzzySharp.Fuzz.PartialRatio(normalizedSentence, query);

                if (similarityScore > bestMatchScore)
                {
                    bestMatchScore = similarityScore;
                    bestMatchSentence = sentence.Trim();
                }

                var sentenceWords = normalizedSentence.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var queryWords = query.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in queryWords)
                {
                    if (sentenceWords.Contains(word) && similarityScore > bestMatchScore)
                    {
                        bestMatchScore = similarityScore;
                        bestMatchSentence = sentence.Trim();
                    }
                }
            }

            if (bestMatchScore >= 50)
            {
                return new Tuple<string, string>(bestMatchSentence, bestMatchSentence);
            }

            return null;
        }

        private string NormalizeQuery(string query)
        {
            // Normalize query by replacing synonyms with a standard term
            foreach (var entry in SynonymService.Synonyms) // Access the synonyms from the service
            {
                foreach (var synonym in entry.Value)
                {
                    if (query.Contains(synonym))
                    {
                        query = query.Replace(synonym, entry.Key);
                    }
                }
            }

            // Remove any special characters, if needed
            query = Regex.Replace(query, @"[^a-zA-Z\s]", "").ToLower();
            return query;
        }

        // Extracts frequent terms from document (You may use a frequency analyzer for better term extraction)
        private List<string> ExtractFrequentTerms(string documentText)
        {
            var wordFrequency = new Dictionary<string, int>();
            var words = documentText.Split(new[] { ' ', '.', ',', ';', ':', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var normalizedWord = word.ToLower();
                if (!wordFrequency.ContainsKey(normalizedWord))
                {
                    wordFrequency[normalizedWord] = 1;
                }
                else
                {
                    wordFrequency[normalizedWord]++;
                }
            }

            return wordFrequency.OrderByDescending(kv => kv.Value)
                                 .Take(10) // Return top 10 frequent terms, can be adjusted
                                 .Select(kv => kv.Key)
                                 .ToList();
        }


        private (string Answer, string Context, List<string> RelatedQuestions) FindAnswerAndContextWithSuggestions(
          string documentText,
          string question,
          List<Question> questions)
        {
            string lowerCaseDocument = documentText.ToLower();
            string lowerCaseQuestion = question.ToLower();

            var relevantSentences = ExtractRelevantSentences(lowerCaseDocument, lowerCaseQuestion);

            if (relevantSentences.Any())
            {
                string answer = GenerateFocusedAnswer(relevantSentences, question);
                string context = string.Join("\n", relevantSentences);

                return (Answer: answer, Context: context, RelatedQuestions: new List<string>());
            }
            else
            {
                var relatedQuestions = FindRelatedQuestions(questions, question);

                return (Answer: "Sorry, I couldn't find a relevant answer to your question.", Context: "", RelatedQuestions: relatedQuestions);
            }
        }

        private List<string> ExtractRelevantSentences(string documentText, string question)
        {
            var relevantSentences = new List<string>();

            var sentences = documentText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sentence in sentences)
            {
                if (ContainsQuestionPhrase(sentence, question))
                {
                    relevantSentences.Add(sentence.Trim());
                }
            }

            return relevantSentences;
        }

        private bool ContainsQuestionPhrase(string sentence, string question)
        {
            string normalizedSentence = sentence.ToLower();
            string normalizedQuestion = question.ToLower();

            return normalizedSentence.Contains(normalizedQuestion) ||
                   HasCommonKeywords(normalizedSentence, normalizedQuestion);
        }

        private bool HasCommonKeywords(string sentence, string question)
        {
            var sentenceWords = sentence.Split(' ');
            var questionWords = question.Split(' ');

            return sentenceWords.Intersect(questionWords).Count() > 1;
        }


        private string GenerateFocusedAnswer(List<string> relevantSentences, string question)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Here’s what I found for your question: '{question}'");

            foreach (var sentence in relevantSentences)
            {
                sb.AppendLine("- " + sentence);
            }

            return sb.ToString();
        }


        private List<string> FindRelatedQuestions(List<Question> questions, string query)
        {
            var relatedQuestions = new List<string>();

            foreach (var question in questions)
            {
                int similarity = FuzzySharp.Fuzz.PartialRatio(question.Text, query);

                if (similarity > 45)
                {
                    relatedQuestions.Add(question.Text);
                }
            }

            return relatedQuestions.Take(5).ToList();
        }

        private Question FindBestVectorMatch(List<Question> questions, string query)
        {
            var queryVector = GenerateOneHotVector(query);

            double bestScore = 0;
            Question bestMatch = null;

            foreach (var question in questions)
            {
                var questionVector = GenerateOneHotVector(question.Text);
                double similarity = ComputeCosineSimilarity(queryVector, questionVector);

                if (similarity > bestScore)
                {
                    bestScore = similarity;
                    bestMatch = question;
                }
            }

            return bestScore > 0.7 ? bestMatch : null;
        }

        private static double ComputeCosineSimilarity(int[] vector1, int[] vector2)

        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must be of the same length.");

            int dotProduct = 0;
            int magnitude1 = 0;
            int magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }



        private static int[] GenerateOneHotVector(string text)
        {
            const int vectorSize = 10000; // Adjust the vector size based on the vocabulary size
            int[] vector = new int[vectorSize];

            foreach (char c in text)
            {
                if (c < vectorSize)
                {
                    vector[(int)c]++;
                }
            }

            return vector;
        }




        private List<Question> GetGeneratedQuestions(string documentText)
        {
            var questionGenerator = new QuestionGenerator();
            return questionGenerator.GenerateQuestionsFromText(documentText);
        }



        private string ExtractTextFromDocx(string documentPath)
        {
            StringBuilder documentText = new StringBuilder();

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(documentPath, false))
            {
                // Extract text from the main document body (paragraphs)
                foreach (var paragraph in wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>())
                {
                    AppendParagraphText(paragraph, documentText);
                }

                // Extract text from headers
                foreach (var headerPart in wordDoc.MainDocumentPart.HeaderParts)
                {
                    var header = (Header)headerPart.RootElement;
                    foreach (var paragraph in header.Elements<Paragraph>())
                    {
                        AppendParagraphText(paragraph, documentText);
                    }
                }

                // Extract text from footers
                foreach (var footerPart in wordDoc.MainDocumentPart.FooterParts)
                {
                    var footer = (Footer)footerPart.RootElement;
                    foreach (var paragraph in footer.Elements<Paragraph>())
                    {
                        AppendParagraphText(paragraph, documentText);
                    }
                }

                // Extract text from footnotes (if they exist)
                if (wordDoc.MainDocumentPart.FootnotesPart != null)
                {
                    var footnotes = (Footnotes)wordDoc.MainDocumentPart.FootnotesPart.RootElement;
                    foreach (var footnote in footnotes.Elements<DocumentFormat.OpenXml.Wordprocessing.Footnote>())
                    {
                        foreach (var paragraph in footnote.Elements<Paragraph>())
                        {
                            AppendParagraphText(paragraph, documentText);
                        }
                    }
                }

                // Extract text from tables in the main body
                foreach (var table in wordDoc.MainDocumentPart.Document.Body.Elements<Table>())
                {
                    AppendTableText(table, documentText);
                }
            }

            return documentText.ToString();
        }

        // Helper method to extract text from paragraphs
        private void AppendParagraphText(Paragraph paragraph, StringBuilder documentText)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = run.Elements<Text>().FirstOrDefault()?.Text;
                if (text != null)
                {
                    documentText.Append(text + " ");
                }
            }
        }

        // Helper method to extract text from tables
        private void AppendTableText(Table table, StringBuilder documentText)
        {
            foreach (var row in table.Elements<TableRow>())
            {
                foreach (var cell in row.Elements<TableCell>())
                {
                    // Extract text from all paragraphs within the cell
                    foreach (var paragraph in cell.Elements<Paragraph>())
                    {
                        AppendParagraphText(paragraph, documentText);
                    }
                }
            }
        }

        private void ExtractImagesFromDocument(string documentPath)
        {
            _imageData.Clear();

            using (FileStream fileStream = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
            using (WordDocument document = new WordDocument(fileStream, FormatType.Docx))
            {
                foreach (WSection section in document.Sections)
                {
                    foreach (WParagraph paragraph in section.Body.ChildEntities.OfType<WParagraph>())
                    {
                        string nearbyText = paragraph.Text.Trim(); // Get the paragraph text

                        foreach (WPicture picture in paragraph.ChildEntities.OfType<WPicture>())
                        {
                            // Attempt to find a caption for the image
                            string captionText = GetImageCaption(paragraph, document);

                            // Combine the nearby text and caption text
                            string combinedText = string.IsNullOrEmpty(captionText)
                                ? nearbyText
                                : $"{nearbyText} (Caption: {captionText})";

                            // Convert the image to Base64 string
                            string base64Image = Convert.ToBase64String(picture.ImageBytes);

                            // Store the image data and combined text
                            _imageData.Add(new ImageWithText
                            {
                                Base64Image = base64Image,
                                NearbyText = combinedText
                            });
                        }
                    }
                }
            }
        }


        // Helper method to retrieve the caption for an image
        private string GetImageCaption(WParagraph imageParagraph, WordDocument document)
        {
            // Find the next paragraph or nearby paragraph with potential caption
            WParagraph nextParagraph = imageParagraph.OwnerTextBody
                .ChildEntities
                .OfType<WParagraph>()
                .SkipWhile(p => p != imageParagraph) // Skip paragraphs until the image paragraph
                .Skip(1) // Go to the next paragraph
                .FirstOrDefault();

            // Check if the next paragraph contains text that looks like a caption
            return nextParagraph?.Text.Trim();
        }



    }
}




