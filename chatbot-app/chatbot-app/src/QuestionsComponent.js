import React, { useEffect, useState } from "react";
import axios from "axios";

const QuestionsComponent = () => {
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [visibleCount, setVisibleCount] = useState(3);
  const [selectedContext, setSelectedContext] = useState(null);

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

  const handleLoadMore = () => {
    setVisibleCount((prevCount) => prevCount + 3);
  };

  const handleQuestionClick = async (question) => {
    try {
      const response = await axios.post("http://localhost:5197/api/Chatbot/ask-question", {
        question,
      });
      setSelectedContext(response.data);
    } catch (error) {
      setSelectedContext({ answer: "Failed to fetch context. Please try again." });
    }
  };

  if (loading) {
    return <div>Loading questions...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div className="questions">
      <h2>Generated Questions</h2>
      <ul>
        {questions.slice(0, visibleCount).map((question, index) => (
          <li key={index} onClick={() => handleQuestionClick(question.text)}>
            {question.text}
          </li>
        ))}
      </ul>
      {visibleCount < questions.length && (
        <button onClick={handleLoadMore}>Load More</button>
      )}
      {selectedContext && (
        <div className="context">
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
    </div>
  );
};

export default QuestionsComponent;
