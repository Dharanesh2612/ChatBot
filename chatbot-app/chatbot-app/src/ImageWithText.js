// import React, { useState } from "react";
// import axios from "axios";
// import "./ImageWithText.css"; // Optional for custom styling

// const ImageWithText = () => {
//   const [images, setImages] = useState([]);
//   const [searchText, setSearchText] = useState("");
//   const [message, setMessage] = useState("");

//   // Fetch all images
//   const fetchAllImages = async () => {
//     try {
//       const response = await axios.get(
//         "http://localhost:5197/api/document-images/all-images"
//       );
//       if (response.data.images) {
//         setImages(response.data.images);
//         setMessage("");
//       } else {
//         setImages([]);
//         setMessage(response.data.message);
//       }
//     } catch (error) {
//       console.error(error);
//       setMessage("Error fetching images. Please try again later.");
//     }
//   };

//   // Search images by text
//   const searchImages = async () => {
//     if (!searchText) {
//       setMessage("Please enter text to search.");
//       return;
//     }

//     try {
//       const response = await axios.post(
//         "http://localhost:5197/api/document-images/search-images",
//         JSON.stringify(searchText),
//         {
//           headers: { "Content-Type": "application/json" },
//         }
//       );
//       if (response.data.images) {
//         setImages(response.data.images);
//         setMessage("");
//       } else {
//         setImages([]);
//         setMessage(response.data.message);
//       }
//     } catch (error) {
//       console.error(error);
//       setMessage("Error searching for images. Please try again later.");
//     }
//   };

//   return (
//     <div className="image-with-text-container">
//       <h1>Document Images</h1>

//       {/* Search Bar */}
//       <div className="search-bar">
//         <input
//           type="text"
//           placeholder="Search text..."
//           value={searchText}
//           onChange={(e) => setSearchText(e.target.value)}
//         />
//         <button onClick={searchImages}>Search</button>
//         <button onClick={fetchAllImages}>Show All Images</button>
//       </div>

//       {/* Message Display */}
//       {message && <p className="message">{message}</p>}

//       {/* Images Display */}
//       <div className="image-grid">
//         {images.map((image, index) => (
//           <div key={index} className="image-card">
//             {/* Display Image */}
//             <img
//                          src={`data:image/png;Base64,${image.Base64Image}`}
//                          alt={`Image ${index + 1}`}
//                          className="image"
//                        />
//             {/* Display Nearby Text */}
//             <p className="nearby-text">{image.nearbyText}</p>
//           </div>
//         ))}
//       </div>
//     </div>
//   );
// };

// export default ImageWithText;



import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ImageWithText = () => {
  const [searchText, setSearchText] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [error, setError] = useState('');
  
  // Function to search for images based on text
  const handleSearch = async () => {
    if (!searchText.trim()) {
        setError('Search text cannot be empty.');
        return;
    }
    setError(''); // Clear previous errors if any

    try {
        const response = await axios.post(
            'http://localhost:5197/api/document-images/search-images', 
            searchText, 
            {
                headers: {
                    'Content-Type': 'application/json', // Set content type to application/json
                }
            }
        );

        // Log the response to check the data format
        console.log('Search response:', response.data);

        if (response.data.images) {
            // If the response contains images, update state
            setSearchResults(response.data.images);
        } else if (response.data.message) {
            setError(response.data.message); // Show message if no images found
        } else {
            setError('Unexpected response format from backend.');
        }
    } catch (error) {
        console.error('Error searching for images:', error);
        setError('Failed to search for images. Check the console for more details.');
    }
};


  return (
    <div>
      <h1>Searcsh Document Images with Text</h1>

      {/* Search Section */}
      <div>
        <input
          type="text"
          placeholder="Enter search text"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
        />
        <button onClick={handleSearch}>Search</button>
      </div>

      {/* Display Error */}
      {error && <p style={{ color: 'red' }}>{error}</p>}

      {/* Display Search Results */}
      <div>
        <h2>Search Results</h2>
        {searchResults.length > 0 ? (
          searchResults.map((result, index) => (
            <div key={index}>
              <h3>{result.NearbyText}</h3>

              {/* Render the base64 image */}
              {result.Base64Images && result.Base64Images.length > 0 ? (
                result.Base64Images.map((base64Image, idx) => (
                  <div key={idx}>
                    <img
                      src={base64Image}  // Already contains 'data:image/png;base64,' prefix
                      alt={`Image ${index + 1}`}
                      style={{ maxWidth: '100%', marginBottom: '10px' }}
                    />
                  </div>
                ))
              ) : (
                <p>No valid image found.</p>
              )}
            </div>
          ))
        ) : (
          <p>No images found for the given search text.</p>
        )}
      </div>
    </div>
  );
};

export default ImageWithText;
