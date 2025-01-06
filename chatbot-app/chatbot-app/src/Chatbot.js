import React, { useState, useEffect } from "react";
import axios from "axios";
import { FaPaperPlane } from 'react-icons/fa'; // To display the send icon
import "./Chatbot.css"; // Include chatbot styling

// QuestionsComponent: Displays a list of questions that the user can click on
const QuestionsComponent = ({ onQuestionClick }) => {
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios
      .get("http://localhost:5197/api/Chatbot/get-questions")
      .then((response) => {
        setQuestions(response.data);
        setLoading(false);
      })
      .catch((error) => {
        setError(error.message);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <div>Loading questions...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div className="chatbot-questions">
      <ul>
        {questions.map((question, index) => (
          <li key={index} onClick={() => onQuestionClick(question.text)}>
            {question.text}
          </li>
        ))}
      </ul>
    </div>
  );
};

// AskQuestionComponent: Allows users to type and submit their own questions
const AskQuestionComponent = ({ onSubmit }) => {
  const [question, setQuestion] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(question);
    setQuestion(""); // Clear the input field
  };

  return (
    <div className="chatbot-ask-question">
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          placeholder="Type your question..."
        />
        <button type="submit" className="send-btn">
          <FaPaperPlane size={20} />
        </button>
      </form>
    </div>
  );
};

// Main Chatbot Component that integrates both
const Chatbot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedContext, setSelectedContext] = useState(null);
  const [userQuestion, setUserQuestion] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const toggleChatbot = () => {
    setIsOpen(!isOpen);
    setSelectedContext(null); // Reset selected question when toggling
    setUserQuestion(null); // Clear the user question when closing
  };

  const handleQuestionClick = async (question) => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.post("http://localhost:5197/api/Chatbot/ask-question", {
        question,
      });
      setSelectedContext(response.data);
    } catch (error) {
      setError("Failed to fetch context. Please try again.");
    }
    setLoading(false);
  };

  const handleAskQuestionSubmit = async (question) => {
    setLoading(true);
    setError(null);
    setUserQuestion(question); // Set user question to display in chat
    try {
      const response = await axios.post("http://localhost:5197/api/Chatbot/ask-question", {
        question,
      });
      setSelectedContext(response.data);
    } catch (error) {
      setError("Failed to fetch context. Please try again.");
    }
    setLoading(false);
  };

  return (
    <div className="chatbot-container">
      <button className="chatbot-toggle-btn" onClick={toggleChatbot}>
        {isOpen ? "Close Chatbot" : "Open Chatbot"}
      </button>

      {isOpen && (
        <div className="chatbot-box">
          <div className="chatbot-header">
            <h2>Welcome! How can I assist you today?</h2>
          </div>

          <div className="chatbot-body">
            {/* Predefined Question Suggestions */}
            <QuestionsComponent onQuestionClick={handleQuestionClick} />

            {/* Ask a Question Input Box */}
            <AskQuestionComponent onSubmit={handleAskQuestionSubmit} />

            {/* Display Loading State */}
            {loading && <div>Loading...</div>}

            {/* Display Answer */}
            {selectedContext && (
              <div className="chatbot-response">
                <h3>Answer:</h3>
                <p>{selectedContext.answer}</p>
                {selectedContext.context && (
                  <>
                    <h4>Context:</h4>
                    <p>{selectedContext.context}</p>
                  </>
                )}
              </div>
            )}

            {/* Display User Question and Answer if provided */}
            {userQuestion && !loading && (
              <div className="chatbot-user-question">
                <h4>You asked:</h4>
                <p>{userQuestion}</p>
                <h4>Answer:</h4>
                {error ? <p>Error: {error}</p> : <p>{selectedContext ? selectedContext.answer : ""}</p>}
              </div>
            )}

            {error && <div className="chatbot-error">Error: {error}</div>}
          </div>
        </div>
      )}
    </div>
  );
};

export default Chatbot;
