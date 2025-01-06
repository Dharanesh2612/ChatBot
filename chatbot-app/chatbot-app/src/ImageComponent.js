import React, { useState } from "react";
import axios from "axios";
// import "./ImageComponent.css"; // Optional CSS for styling

const ImageComponent = () => {
  const [question, setQuestion] = useState("");
  const [response, setResponse] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleInputChange = (e) => {
    setQuestion(e.target.value);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!question.trim()) {
      setError("Please enter a valid question.");
      return;
    }
    setLoading(true);
    setError("");
    setResponse(null);

    try {
      const res = await axios.post("http://localhost:5197/api/Image/ask-question", {
        question,
      });
      setResponse(res.data);
    } catch (err) {
      setError(err.response?.data || "An error occurred while fetching the answer.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="image-component">
      <h1>Ask a Question</h1>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Enter your question"
          value={question}
          onChange={handleInputChange}
          className="question-input"
        />
        <button type="submit" className="submit-button">
          {loading ? "Loading..." : "Ask"}
        </button>
      </form>
      {error && <div className="error-message">{error}</div>}
      {response && (
        <div className="response-container">
          <h2>Answer:</h2>
          <p>{response.answer}</p>
          {response.context && (
            <>
              <h3>Context:</h3>
              <p>{response.context}</p>
            </>
          )}
          {response.relatedQuestions && response.relatedQuestions.length > 0 && (
            <>
              <h3>Related Questions:</h3>
              <ul>
                {response.relatedQuestions.map((q, index) => (
                  <li key={index}>{q}</li>
                ))}
              </ul>
            </>
          )}
          {response.images && response.images.length > 0 && (
            <>
              <h3>Images:</h3>
              <div className="images-container">
                {response.images.map((img, index) => (
                  <img key={index} src={img} alt={`Document Image ${index + 1}`} />
                ))}
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
};

export default ImageComponent;
