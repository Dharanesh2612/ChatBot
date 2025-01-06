

//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using FuzzySharp;
//using ChatbotAPI.Models;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;

//namespace ChatbotAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ImageController : ControllerBase
//    {
//        [HttpPost("ask-question")]
//        public IActionResult AskQuestion([FromBody] UserQuery query)
//        {
//            if (query == null || string.IsNullOrWhiteSpace(query.Question))
//            {
//                return BadRequest("Please provide a valid question.");
//            }

//            // Assume _documentPath and ExtractTextFromDocx are available from shared service or utility
//            string documentPath = "C:\\Users\\dharanesh.r\\Documents\\Creating a claim application is a skill that requires clarity.docx";
//            string documentText = ExtractTextFromDocx(documentPath);

//            var questionGenerator = new QuestionGenerator();
//            List<Question> questions = questionGenerator.GenerateQuestionsFromText(documentText);

//            // Step 1: Check for exact or close match with pre-generated questions
//            var matchedQuestion = FindExactOrCloseMatch(questions, query.Question);
//            if (matchedQuestion != null)
//            {
//                return Ok(new
//                {
//                    answer = matchedQuestion.Text,
//                    context = matchedQuestion.Context,
//                    relatedQuestions = new List<string>()
//                });
//            }

//            // Step 2: Fallback to extracting relevant answer from the document
//            var result = FindAnswerAndContextWithSuggestions(documentText, query.Question, questions);

//            return Ok(new
//            {
//                answer = result.Answer,
//                context = result.Context,
//                relatedQuestions = result.RelatedQuestions
//            });
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

//        private Question FindExactOrCloseMatch(List<Question> questions, string query)
//        {
//            foreach (var question in questions)
//            {
//                // Check for exact match
//                if (question.Text.Equals(query, StringComparison.OrdinalIgnoreCase))
//                {
//                    return question;
//                }

//                // Check for close match using similarity score
//                int similarity = FuzzySharp.Fuzz.PartialRatio(question.Text, query);
//                if (similarity > 100) // Adjust threshold as needed
//                {
//                    return question;
//                }
//            }

//            return null;
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

//        private List<string> ExtractRelevantSentences(string documentText, string question)
//        {
//            var relevantSentences = new List<string>();

//            var sentences = documentText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

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
//    }
//}

using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FuzzySharp;
using ChatbotAPI.Models;

namespace ChatbotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("ask-question")]
        public IActionResult AskQuestion([FromBody] UserQuery query)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Question))
            {
                return BadRequest("Please provide a valid question.");
            }

            string documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
            string documentText = ExtractTextFromDocx(documentPath, out List<string> images);

            var questionGenerator = new QuestionGenerator();
            List<Question> questions = questionGenerator.GenerateQuestionsFromText(documentText);

            // Step 1: Check for exact or close match with pre-generated questions
            var matchedQuestion = FindExactOrCloseMatch(questions, query.Question);
            if (matchedQuestion != null)
            {
                return Ok(new
                {
                    answer = matchedQuestion.Text,
                    context = matchedQuestion.Context,
                    images
                });
            }

            // Step 2: Fallback to extracting relevant answer from the document
            var result = FindAnswerAndContextWithSuggestions(documentText, query.Question, questions);

            return Ok(new
            {
                answer = result.Answer,
                context = result.Context,
                images,
                relatedQuestions = result.RelatedQuestions
            });
        }

        private string ExtractTextFromDocx(string documentPath, out List<string> images)
        {
            StringBuilder documentText = new StringBuilder();
            images = new List<string>();

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(documentPath, false))
            {
                foreach (var paragraph in wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>())
                {
                    foreach (var run in paragraph.Elements<Run>())
                    {
                        var text = run.Elements<Text>().FirstOrDefault()?.Text;
                        if (text != null)
                        {
                            documentText.Append(text + " ");
                        }

                        // Check for embedded images
                        var drawing = run.Elements<Drawing>().FirstOrDefault();
                        if (drawing != null)
                        {
                            var imagePart = GetImageFromDrawing(wordDoc, drawing);
                            if (imagePart != null)
                            {
                                // Convert image to Base64 and add to the list
                                images.Add(ConvertImageToBase64(imagePart));
                            }
                        }
                    }
                }
            }

            return documentText.ToString();
        }

        private ImagePart GetImageFromDrawing(WordprocessingDocument wordDoc, Drawing drawing)
        {
            var blip = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
            if (blip != null)
            {
                string relationshipId = blip.Embed.Value;
                return wordDoc.MainDocumentPart.GetPartById(relationshipId) as ImagePart;
            }

            return null;
        }

        private string ConvertImageToBase64(ImagePart imagePart)
        {
            using (var stream = imagePart.GetStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return $"data:image/{imagePart.ContentType.Split('/')[1]};base64," +
                       Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private Question FindExactOrCloseMatch(List<Question> questions, string query)
        {
            foreach (var question in questions)
            {
                if (question.Text.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return question;
                }

                int similarity = FuzzySharp.Fuzz.PartialRatio(question.Text, query);
                if (similarity > 100) // Adjust threshold as needed
                {
                    return question;
                }
            }

            return null;
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

            var sentences = documentText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

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

                if (similarity > 60)
                {
                    relatedQuestions.Add(question.Text);
                }
            }

            return relatedQuestions.Take(5).ToList();
        }
    }
}
