


import React, { useState, useEffect } from "react";
import axios from "axios";
import { FaPaperPlane } from "react-icons/fa"; // To display the send icon
import { FaSyncAlt } from "react-icons/fa";
import "./Chatbot.css"; // Include chatbot styling


const QuestionsComponent = ({ onQuestionClick }) => {
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [visibleCount, setVisibleCount] = useState(3);

  useEffect(() => {
    axios
      .get("http://localhost:5197/api/vector-chatbot/get-questions")
      .then((response) => {
        setQuestions(response.data);
        setLoading(false);
      })
      .catch((error) => {
        setError(error.message);
        setLoading(false);
      });
  }, []);

  const handleShowMore = () => {
    setVisibleCount((prevCount) => prevCount + 3); // Show 3 more questions each time
  };

  if (loading) {
    return <div>Loading questions...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div className="chatbot-questions">
      <ul>
        {questions.slice(0, visibleCount).map((question, index) => (
          <li key={index} onClick={() => onQuestionClick(question)}>
            {question.text}
          </li>
        ))}
      </ul>

      {questions.length > visibleCount && (
        <button onClick={handleShowMore} className="see-more-btn">
          See More
        </button>
      )}
    </div>
  );
};

// Component to handle user-input questions
const AskQuestionComponent = ({ onSubmit }) => {
  const [question, setQuestion] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!question.trim()) return;
    onSubmit(question);
    setQuestion("");
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

// Main Chatbot Component
const Chatbot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isFullScreen, setIsFullScreen] = useState(true); // Default to full-screen mode
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isQuestionAnswered, setIsQuestionAnswered] = useState(false); // Track question-answer state

  const toggleChatbot = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      
      setIsFullScreen(true); 
    }
    setMessages([]); 
  };
  
  const toggleFullScreen = () => {
    if (isFullScreen) {
      // close the chatbot
      setIsFullScreen(false);
      setIsOpen(false); //close the chatbot fully
    } else {
      // Enter full-screen mode
      setIsFullScreen(true);
    }
  };
  

  
  const handleQuestionClick = async (question) => {
    setIsQuestionAnswered(true); 
    await handleQuestionSubmit(question.text);
    setIsQuestionAnswered(false); // Show predefined questions again after response
  };

  
  const handleAskQuestionSubmit = async (question) => {
    setIsQuestionAnswered(true); 
    await handleQuestionSubmit(question);
    setIsQuestionAnswered(false); 
  };

  const handleQuestionSubmit = async (question) => {
    setMessages((prevMessages) => [...prevMessages, { text: question, sender: "user" }]);
    setLoading(true);
    setError(null);

    try {
      const response = await axios.post("http://localhost:5197/api/vector-chatbot/ask-vector-question", { question });

      const botResponse = response.data.answer; 
      const images = response.data.images?.[0].images || []; 

      
      const base64Images = images.map((image) => image); 

      // Update the state with the response data and images
      setMessages((prevMessages) => [
        ...prevMessages,
        { text: botResponse, sender: "bot" },
        ...(base64Images.length > 0 ? [{ text: "", sender: "bot", images: base64Images }] : []),
      ]);
    } catch (error) {
      console.error("Error fetching response:", error);
      setError("Failed to fetch response. Please try again.");
    }

    setLoading(false);
  };

  return (
    <div className={`chatbot-container ${isFullScreen ? "full-screen" : ""}`}>
      {/* Show either Open or Close button based on isOpen state */}
      {!isOpen && (
        <button className="chatbot-toggle-btn" onClick={toggleChatbot}>
          Open Chatbot
        </button>
      )}

      {isOpen && (
        <div className="chatbot-box">
          <div className="chatbot-header">
            <h2>Welcome to Avant! Feel free to ask me anything!</h2>
            <button className="fullscreen-btn" onClick={toggleFullScreen}>
              {isFullScreen ? "Exit Screen" : "Full-Screen"}
            </button>

           
            <button className="refresh-btn" onClick={() => setMessages([])}>
              <FaSyncAlt size={20} />
            </button>
          </div>

          <div className="chatbot-body">
            <div className="chatbot-messages">
              {messages.map((message, index) => (
                <div key={index} className={message.sender === "user" ? "user-message" : "bot-message"}>
                  <p>{message.text}</p>
                          {message.images && message.images.length > 0 && (
                    <div className="image-container">
                      {message.images.map((image, idx) => (
                        <img
                          key={idx}
                          src={`data:image/jpeg;base64,${image}`}
                          alt={`Image ${idx + 1}`}
                          className="image"
                          style={{ width: "200px", height: "200px", margin: "10px" }}
                        />
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>

            <AskQuestionComponent onSubmit={handleAskQuestionSubmit} />
            {/* {!isQuestionAnswered && (
               <QuestionsComponent onQuestionClick={handleQuestionClick} />
            )} */}
            {loading && <div>Loading...</div>}
            {error && <div className="chatbot-error">Error: {error}</div>}
          </div>
        </div>
      )}
    </div>
  );
};

export default Chatbot;




