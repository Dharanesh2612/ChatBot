


import React, { useState } from "react";
import axios from "axios";

const AskQuestionComponent = () => {
  const [question, setQuestion] = useState("");
  const [response, setResponse] = useState(null);
  const [error, setError] = useState(null);

  const handleSubmit = (e) => {
    e.preventDefault();

    axios
      .post("http://localhost:5197/api/Chatbot/ask-question", { question })
      .then((response) => {
        setResponse(response.data);
        setError(null);
      })
      .catch((error) => {
        setError(error.message);
        setResponse(null);
      });
  };

  const handleRelatedQuestionClick = (relatedQuestion) => {
    axios
      .post("http://localhost:5197/api/Chatbot/ask-question", { question: relatedQuestion })
      .then((response) => {
        setResponse(response.data);
        setError(null);
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  return (
    <div className="ask-question">
      <h2>Ask a Question</h2>
      <form onSubmit={handleSubmit}>
        <textarea
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          placeholder="Type your question here..."
          rows="5"
        ></textarea>
        <button type="submit">Submit</button>
      </form>
      {response && (
        <div className="response">
          <h3>Answer:</h3>
          <p>{response.answer}</p>
          {response.context && (
            <div>
              <h4>Context:</h4>
              <p>{response.context}</p>
            </div>
          )}
          {response.relatedQuestions.length > 0 && (
            <div>
              <h4>Related Questions:</h4>
              <ul>
                {response.relatedQuestions.map((rq, index) => (
                  <li
                    key={index}
                    style={{ cursor: "pointer", color: "blue", textDecoration: "underline" }}
                    onClick={() => handleRelatedQuestionClick(rq)}
                  >
                    {rq}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
      {error && <div className="error">Error: {error}</div>}
    </div>
  );
};

export default AskQuestionComponent;
