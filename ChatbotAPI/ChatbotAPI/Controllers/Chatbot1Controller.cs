

using Google.Cloud.Dialogflow.V2;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatbotAPI.Models;
using ChatbotAPI.Services;
using FuzzySharp;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;


namespace ChatbotAPI.Controllers
{
    [Route("api/chatbot1")]
    [ApiController]
    public class Chatbot1Controller : ControllerBase
    {
        private static string _documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
        private static List<string> _vocabulary = new List<string>();
        private static List<ImageWithText> _imageData = new List<ImageWithText>();

        private static Dictionary<string, string> _fixedTextToImage = new Dictionary<string, string>
        {
            { "claims create,create claims,claims,what are does claims do,Can you explain claims?", "Image_1.png,Image_2.png" },
            { "manual check,check manual", "Image_6.png" },
            { "Create and Fill the Claim Form,how to create and fill the claim form", "Image_2.png" },
            { "Medical Service Information,Automatic Data Validation" ,"Image_4.png"},
            { "Billing and Insurance Information", "Image_5.png" },
            { "Post Submission and Follow Up", "Image_2.png,Image_4.png" },
            { "Required Documents,Documents required","Image_1.png" },
        };

        [HttpPost("ask-vector-question")]
        public IActionResult AskVectorQuestion([FromBody] UserQuery query)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Question))
            {
                return BadRequest("Please provide a valid question.");
            }

            // Use Dialogflow API to handle user query
            var dialogflowResponse = GetDialogflowResponse(query.Question);

            if (!string.IsNullOrEmpty(dialogflowResponse))
            {
                return Ok(new
                {
                    answer = dialogflowResponse,
                    context = "Dialogflow Context Here",
                    images = (object)null
                });
            }

            // Fall back to your existing logic if Dialogflow does not provide an answer
            string normalizedQuery = NormalizeQuery(query.Question.ToLower());
            string documentText = ExtractTextFromDocx(_documentPath);
            var questions = GetGeneratedQuestions(documentText);

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

            // Handle image and fixed text matches if found
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

            var imageResults = _imageData
                .Where(data => Fuzz.PartialRatio(data.NearbyText, normalizedQuery) >= similarityThreshold)
                .Select(data => new { data.Base64Image, data.NearbyText })
                .ToList();

            return Ok(new
            {
                answer = result.Answer + ".",
                context = result.Context,
                images = imageResults
            });
        }

        private string GetDialogflowResponse(string userQuery)
        {
            try
            {
                // Load credentials from the service account JSON file
                var credential = GoogleCredential.FromFile("C:\\Users\\dharanesh.r\\Downloads\\my-project-29639-450510-de814cb8e81e.json");
                
                // Create the Dialogflow session client with the credentials
                var sessionClient = new SessionsClientBuilder
                {
                    Credential = credential // Set the credentials here
                }.Build();  // Build the client instance

                // Set up the session (replace 'unique-session-id' with a real session ID)
                var session = SessionName.FromProjectSession("my-project-29639-450510", "unique-session-id");

                // Set up the text input
                var textInput = new Google.Cloud.Dialogflow.V2.TextInput
                {
                    Text = userQuery,
                    LanguageCode = "en"
                };

                // Create the query input
                var queryInput = new QueryInput
                {
                    Text = textInput
                };

                // Call Dialogflow API to get the response
                var response = sessionClient.DetectIntent(session, queryInput);

                // Check if Dialogflow provides an answer
                if (!string.IsNullOrEmpty(response.QueryResult.FulfillmentText))
                {
                    // If Dialogflow gives a valid response, return it
                    return response.QueryResult.FulfillmentText;
                }

                // If Dialogflow doesn't provide an answer, fallback to document-based search
                string normalizedQuery = NormalizeQuery(userQuery.ToLower());
                string documentText = ExtractTextFromDocx(_documentPath);
                var questions = GetGeneratedQuestions(documentText);

                // Find answer and context using the document
                var result = FindAnswerAndContextWithSuggestions(documentText, normalizedQuery, questions);

                // Return the document-based answer if Dialogflow doesn't respond with anything meaningful
                return result.Answer;
            }
            catch (Exception ex)
            {
                // Log the error and return a fallback message
                return $"Sorry, something went wrong: {ex.Message}";
            }
        }

        private string NormalizeQuery(string query)
        {
            foreach (var entry in SynonymService.Synonyms)
            {
                foreach (var synonym in entry.Value)
                {
                    if (query.Contains(synonym))
                    {
                        query = query.Replace(synonym, entry.Key);
                    }
                }
            }

            query = Regex.Replace(query, @"[^a-zA-Z\s]", "").ToLower();
            return query;
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
            var sb = new System.Text.StringBuilder();

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

        private List<Question> GetGeneratedQuestions(string documentText)
        {
            var questionGenerator = new QuestionGenerator();
            return questionGenerator.GenerateQuestionsFromText(documentText);
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

                foreach (var word in sentenceWords)
                {
                    if (word.Equals(query))
                    {
                        bestMatchSentence = sentence.Trim();
                        break;
                    }
                }
            }

            return bestMatchSentence != null ? new Tuple<string, string>(bestMatchSentence, "Generated from fuzzy match.") : null;
        }

        private string ExtractTextFromDocx(string filePath)
        {
            using (var doc = WordprocessingDocument.Open(filePath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                var text = body.Descendants<Text>()
                               .Select(t => t.Text)
                               .Aggregate((a, b) => a + " " + b);

                return text;
            }
        }
    }
}




