import logo from './logo.svg';
import './App.css';

import AskQuestionComponent from './AskQuestionComponent';
import QuestionsComponent from './QuestionsComponent';
import Chatbot from './Chatbot';
import ImageComponent from './ImageComponent';
import ImageWithText from './ImageWithText';
import DialogflowChat from './DialogflowChat';


function App() {
  return (
    <div className="App">
      {/* <QuestionsComponent/>
      <AskQuestionComponent/> */}
      {/* <Chatbot/> */}
       {/* <ImageComponent/> */}
       {/* <ImageWithText/> */}
       <DialogflowChat/>
      
    </div>
  );
}

export default App;
