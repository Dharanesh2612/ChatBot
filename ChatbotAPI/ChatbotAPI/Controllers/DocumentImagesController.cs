



//using ChatbotAPI.Models;
//using Microsoft.AspNetCore.Mvc;
//using Syncfusion.DocIO;
//using Syncfusion.DocIO.DLS;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using FuzzySharp;

//namespace ChatbotAPI.Controllers
//{
//    [Route("api/document-images")]
//    [ApiController]
//    public class DocumentImagesController : ControllerBase
//    {
//        private string _documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
//        private static List<ImageWithText> _imageData = new List<ImageWithText>();

//        // Define fixed texts and their associated images
//        private static Dictionary<string, string> _fixedTextToImage = new Dictionary<string, string>
//        {
//            { "claims create,create claims", "Image_1.png,Image_2.png" },  
//            { "manual check,check manual", "Image_4.png," },
//            {"Required Documents","Image_3.png" }

//        };

//        public DocumentImagesController()
//        {
//            ExtractImagesFromDocument(_documentPath);
//        }

//        // Method to extract images with nearby text from the document
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
//                        string nearbyText = paragraph.Text.Trim(); // Get the paragraph text

//                        foreach (WPicture picture in paragraph.ChildEntities.OfType<WPicture>())
//                        {
//                            // Check if image bytes exist
//                            if (picture.ImageBytes == null || picture.ImageBytes.Length == 0)
//                            {
//                                Console.WriteLine("No image bytes found.");
//                                continue; // Skip this image if there are no bytes
//                            }

//                            // Attempt to find a caption for the image
//                            string captionText = GetImageCaption(paragraph, document);

//                            // Combine the nearby text and caption text
//                            string combinedText = string.IsNullOrEmpty(captionText)
//                                ? nearbyText
//                                : $"{nearbyText} (Caption: {captionText})";

//                            // Convert the image to Base64 string
//                            string base64Image = Convert.ToBase64String(picture.ImageBytes);

//                            // Log base64 image length to verify it's not empty
//                            Console.WriteLine($"Base64 Image Length: {base64Image.Length}");

//                            // Store the image data and combined text
//                            _imageData.Add(new ImageWithText
//                            {
//                                Base64Image = base64Image,
//                                NearbyText = combinedText
//                            });
//                        }
//                    }
//                }
//            }
//        }


//        // Helper method to retrieve the caption for an image
//        private string GetImageCaption(WParagraph imageParagraph, WordDocument document)
//        {
//            // Find the next paragraph or nearby paragraph with potential caption
//            WParagraph nextParagraph = imageParagraph.OwnerTextBody
//                .ChildEntities
//                .OfType<WParagraph>()
//                .SkipWhile(p => p != imageParagraph) // Skip paragraphs until the image paragraph
//                .Skip(1) // Go to the next paragraph
//                .FirstOrDefault();

//            // Check if the next paragraph contains text that looks like a caption
//            return nextParagraph?.Text.Trim();
//        }


//        [HttpPost("search-images")]
//        public IActionResult SearchImages([FromBody] string searchText)
//        {
//            if (string.IsNullOrWhiteSpace(searchText))
//            {
//                return BadRequest(new { message = "Search text cannot be empty." });
//            }

//            // Set similarity threshold (e.g., 60%)
//            const int similarityThreshold = 60;

//            // Check if the searchText matches any of the predefined fixed texts
//            var fixedTextResults = _fixedTextToImage
//                .Where(entry => Fuzz.PartialRatio(entry.Key, searchText) >= similarityThreshold)
//                .Select(entry =>
//                {
//                    // Retrieve Base64 image data for the fixed text
//                    var base64Images = entry.Value.Split(',')
//                        .Where(imageName => !string.IsNullOrWhiteSpace(imageName))
//                        .Select(imageName =>
//                        {
//                            var imageData = _imageData.FirstOrDefault(img => $"Image_{_imageData.IndexOf(img) + 1}.png" == imageName);
//                            return imageData?.Base64Image; // Get the actual Base64 image data
//                        })
//                        .Where(img => !string.IsNullOrEmpty(img)) // Remove null/empty images
//                        .ToList();

//                    return new
//                    {
//                        Base64Images = base64Images, // Send all Base64 images matching the fixed text
//                        NearbyText = entry.Key
//                    };
//                })
//                .ToList();

//            // If there's a match for fixed text, return those images first
//            if (fixedTextResults.Any())
//            {
//                return Ok(new { images = fixedTextResults });
//            }

//            // Otherwise, check the document's image data for approximate matches
//            var results = _imageData
//                .Where(data => Fuzz.PartialRatio(data.NearbyText, searchText) >= similarityThreshold) // Approximate matching
//                .Select(data => new { data.Base64Image, data.NearbyText })
//                .ToList();

//            if (!results.Any() && !fixedTextResults.Any())
//            {
//                return Ok(new { message = "No images found for the given text." });
//            }

//            return Ok(new { images = results });
//        }

//        // Endpoint to get all images with their associated text
//        [HttpGet("all-images")]
//        public IActionResult GetAllImages()
//        {
//            if (!_imageData.Any())
//            {
//                return Ok(new { message = "No images found in the document." });
//            }

//            // Return a list of image names with their nearby text
//            var imagesWithNames = _imageData.Select((data, index) => new
//            {
//                ImageName = $"Image_{index + 1}.png", // Assign a unique name for each image
//                data.NearbyText
//            }).ToList();

//            return Ok(new { images = imagesWithNames });
//        }

//    }
//}


using ChatbotAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FuzzySharp;

namespace ChatbotAPI.Controllers
{
    [Route("api/document-images")]
    [ApiController]
    public class DocumentImagesController : ControllerBase
    {
        private string _documentPath = "C:\\Users\\dharanesh.r\\Documents\\Steps to Create and Fill the Doctor.docx";
        private static List<ImageWithText> _imageData = new List<ImageWithText>();

        // Define fixed texts and their associated images
        private static Dictionary<string, string> _fixedTextToImage = new Dictionary<string, string>
        {
            { "claims create,create claims", "Image_1.png,Image_2.png" },
            { "manual check,check manual", "Image_4.png," },
            { "Required Documents", "Image_3.png" }
        };

        public DocumentImagesController()
        {
            ExtractImagesFromDocument(_documentPath);
        }

        // Method to extract images with nearby text from the document
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
                            if (picture.ImageBytes == null || picture.ImageBytes.Length == 0)
                            {
                                Console.WriteLine("No image bytes found.");
                                continue; // Skip this image if there are no bytes
                            }

                            string captionText = GetImageCaption(paragraph, document);
                            string combinedText = string.IsNullOrEmpty(captionText)
                                ? nearbyText
                                : $"{nearbyText} (Caption: {captionText})";

                            // Convert image to Base64
                            //string base64Image = Convert.ToBase64String(picture.ImageBytes);
                            // Convert image to Base64 and add prefix
                            string base64Image = "data:image/png;base64," + Convert.ToBase64String(picture.ImageBytes);


                            // Log Base64 image length
                            Console.WriteLine($"Base64 Image Length: {base64Image.Length}");

                            // Store image data
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
            WParagraph nextParagraph = imageParagraph.OwnerTextBody
                .ChildEntities
                .OfType<WParagraph>()
                .SkipWhile(p => p != imageParagraph) // Skip paragraphs until the image paragraph
                .Skip(1) // Go to the next paragraph
                .FirstOrDefault();

            return nextParagraph?.Text.Trim();
        }

        [HttpPost("search-images")]
        public IActionResult SearchImages([FromBody] string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest(new { message = "Search text cannot be empty." });
            }

            // Set similarity threshold (e.g., 60%)
            const int similarityThreshold = 60;

            var fixedTextResults = _fixedTextToImage
                .Where(entry => Fuzz.PartialRatio(entry.Key, searchText) >= similarityThreshold)
                .Select(entry =>
                {
                    var base64Images = entry.Value.Split(',')
                        .Where(imageName => !string.IsNullOrWhiteSpace(imageName))
                        .Select(imageName =>
                        {
                            var imageData = _imageData.FirstOrDefault(img => $"Image_{_imageData.IndexOf(img) + 1}.png" == imageName);
                            return imageData?.Base64Image; // Get actual Base64 image data
                        })
                        .Where(img => !string.IsNullOrEmpty(img)) // Filter out invalid images
                        .ToList();

                    return new
                    {
                        Base64Images = base64Images,
                        NearbyText = entry.Key
                    };
                })
                .ToList();

            if (fixedTextResults.Any())
            {
                return Ok(new { images = fixedTextResults });
            }

            // Check document's image data for approximate matches
            var results = _imageData
                .Where(data => Fuzz.PartialRatio(data.NearbyText, searchText) >= similarityThreshold)
                .Select(data => new { data.Base64Image, data.NearbyText })
                .ToList();

            if (!results.Any() && !fixedTextResults.Any())
            {
                return Ok(new { message = "No images found for the given text." });
            }

            return Ok(new { images = results });
        }


        // Endpoint to get all images with their associated text
        [HttpGet("all-images")]
        public IActionResult GetAllImages()
        {
            if (!_imageData.Any())
            {
                return Ok(new { message = "No images found in the document." });
            }

            var imagesWithNames = _imageData.Select((data, index) => new
            {
                ImageName = $"Image_{index + 1}.png", // Unique image name
                data.NearbyText
            }).ToList();

            return Ok(new { images = imagesWithNames });
        }
    }
}
