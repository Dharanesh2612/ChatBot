import React, { useState, useEffect, useRef } from "react";
import { X, MessageCircle } from "lucide-react";
import axios from "axios";
import "./DialogflowChat.css"; // Ensure you create this CSS file for styling

const DialogflowChat = () => {
  const [messages, setMessages] = useState([
    { text: "Hello! How can I assist you today?", sender: "bot" },
  ]);
  const [input, setInput] = useState("");
  const [isOpen, setIsOpen] = useState(false);
  const chatEndRef = useRef(null);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userMessage = { text: input, sender: "user" };
    setMessages((prev) => [...prev, userMessage]);

    try {
      const response = await axios.post(
        "https://localhost:7172/api/chatbot1/ask-vector-question",
        { Question: input }
      );
      const botMessage = { text: response.data.answer, sender: "bot" };
      setMessages((prev) => [...prev, botMessage]);

      if (response.data.images?.length) {
        response.data.images.forEach((img) => {
          setMessages((prev) => [
            ...prev,
            { text: "Here’s an image related to your query:", sender: "bot" },
            { image: img.Base64Image, sender: "bot" },
          ]);
        });
      }
    } catch (error) {
      setMessages((prev) => [
        ...prev,
        { text: "Sorry, something went wrong. Please try again.", sender: "bot" },
      ]);
    }

    setInput("");
  };

  return (
    <div>
      {!isOpen && (
        <button className="chat-icon" onClick={() => setIsOpen(true)}>
          <MessageCircle size={24} />
        </button>
      )}

      {isOpen && (
        <div className="dialogflow-chat-container">
          <div className="chat-header">
            <span>Chatbot</span>
            <button className="close-btn" onClick={() => setIsOpen(false)}>
              <X size={24} />
            </button>
          </div>
          <div className="chat-body">
            {messages.map((msg, index) => (
              <div key={index} className={`chat-message ${msg.sender}`}>
                {msg.image ? <img src={`data:image/png;base64,${msg.image}`} alt="Response" /> : msg.text}
              </div>
            ))}
            <div ref={chatEndRef}></div>
          </div>
          <div className="chat-footer">
            <input
              type="text"
              placeholder="Type a message..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyPress={(e) => e.key === "Enter" && sendMessage()}
            />
            <button onClick={sendMessage}>Send</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default DialogflowChat;
