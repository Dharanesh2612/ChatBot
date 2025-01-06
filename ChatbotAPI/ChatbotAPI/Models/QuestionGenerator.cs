


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChatbotAPI.Models
{
    public class QuestionGenerator
    {
        public List<Question> GenerateQuestionsFromText(string documentText)
        {
            var questions = new List<Question>();

            var sentences = Regex.Split(documentText, @"(?<=[.!?])\s+");

            foreach (var sentence in sentences)
            {
                var trimmedSentence = sentence.Trim();

                // 1. Handle More Question Types (Who, When, Where, How, etc.)

                // Handling "Who" questions
                if (trimmedSentence.StartsWith("Who"))
                {
                    var parts = trimmedSentence.Split(new[] { " is " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string subject = parts[0].Trim();
                        string answer = parts[1].Trim();

                        var question = new Question
                        {
                            Text = $"Who is {subject}?",
                            Context = trimmedSentence // Keep the full sentence as context
                        };
                        questions.Add(question);
                    }
                }
                // Handling "When" questions
                else if (trimmedSentence.Contains(" in the year "))
                {
                    var parts = trimmedSentence.Split(new[] { " in the year " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string eventDescription = parts[0].Trim();
                        string year = parts[1].Trim();

                        var question = new Question
                        {
                            Text = $"When did {eventDescription} happen?",
                            Context = trimmedSentence // Keep the full sentence as context
                        };
                        questions.Add(question);
                    }
                }
                // Handling "Where" questions
                else if (trimmedSentence.Contains(" in "))
                {
                    var parts = trimmedSentence.Split(new[] { " in " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string subject = parts[0].Trim();
                        string location = parts[1].Trim();

                        var question = new Question
                        {
                            Text = $"Where did {subject} happen?",
                            Context = trimmedSentence // Keep the full sentence as context
                        };
                        questions.Add(question);
                    }
                }
                // Handling "How" questions
                else if (trimmedSentence.Contains(" are ") && trimmedSentence.Contains(" you "))
                {
                    var parts = trimmedSentence.Split(new[] { " are " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string subject = parts[0].Trim();
                        string answer = parts[1].Trim();

                        var question = new Question
                        {
                            Text = $"How are {subject}?",
                            Context = trimmedSentence // Keep the full sentence as context
                        };
                        questions.Add(question);
                    }
                }

                // 2. Handle Compound Sentences (split and process multiple parts of the sentence)

                // Split sentence into sub-sentences (using commas, semicolons, 'and', 'but')
                var compoundSentences = Regex.Split(trimmedSentence, @"[,;]|\band\b|\bbut\b");

                foreach (var subSentence in compoundSentences)
                {
                    var subTrimmed = subSentence.Trim();
                    // Rule-based question generation for each sub-sentence
                    if (subTrimmed.Contains(" is "))
                    {
                        var parts = subTrimmed.Split(new[] { " is " }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string subject = parts[0].Trim();
                            string answer = parts[1].Trim();

                            var question = new Question
                            {
                                Text = $"What is the {subject}?",
                                Context = trimmedSentence // Keep the full sentence as context
                            };
                            questions.Add(question);
                        }
                    }
                    else if (subTrimmed.Contains(" has "))
                    {
                        var parts = subTrimmed.Split(new[] { " has " }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string subject = parts[0].Trim();
                            string answer = parts[1].Trim();

                            var question = new Question
                            {
                                Text = $"What does {subject} have?",
                                Context = trimmedSentence // Keep the full sentence as context
                            };
                            questions.Add(question);
                        }
                    }
                    else if (subTrimmed.Contains(" are "))
                    {
                        var parts = subTrimmed.Split(new[] { " are " }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string subject = parts[0].Trim();
                            string answer = parts[1].Trim();

                            var question = new Question
                            {
                                Text = $"What are the {subject}?",
                                Context = trimmedSentence // Keep the full sentence as context
                            };
                            questions.Add(question);
                        }
                    }
                }
            }

            return questions;
        }
    }
}

