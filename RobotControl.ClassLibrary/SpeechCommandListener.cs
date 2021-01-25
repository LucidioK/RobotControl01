using System;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;

namespace RobotControl.Net
{
    internal class SpeechCommandListener : ISpeechCommandListener
    {
        private readonly Thread thread;
        SpeechRecognitionEngine speechRecognitionEngine = new SpeechRecognitionEngine();

        GrammarBuilder grammarBuilder = new GrammarBuilder();
        DictationGrammar dictationGrammar = new DictationGrammar();
        string latestText;
        object latestTextLock = new object();
        bool fresh = false;
        string[] commands = new string[] {
            "robot",
            "stop",
            "robot stop",
            "robot continue",
            "robot start",
            "robot go home",
            "robot configure",
            "robot config",
        };


        public SpeechCommandListener(bool fake, IState state)
        {
            this.fake = fake;
            this.state = state;
            foreach (var command in commands) grammarBuilder.Append(command);

            speechRecognitionEngine.SetInputToDefaultAudioDevice();
            speechRecognitionEngine.LoadGrammar(/*new Grammar(grammarBuilder)*/dictationGrammar);
            speechRecognitionEngine.RecognizeCompleted += RecognizeCompleted;
            speechRecognitionEngine.RecognizeAsync();
        }

        public string GetLatestText()
        {
            lock (latestTextLock)
            {
                if (fresh)
                {
                    fresh = false;
                    return latestText;
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly bool fake;
        IState state;
        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

        private void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Result != null && Monitor.TryEnter(latestTextLock))
            {
                foreach (var alternative in e.Result.Alternates)
                {
                    if (commands.Contains(alternative.Text))
                    {
                        latestText = e.Result.Text;
                        pubSub.Publish(new EventDescriptor { Name = EventName.VoiceCommandDetected, Detail = latestText });
                        fresh = true;
                        break;
                    }
                }
            }

            // Restart recognition.
            speechRecognitionEngine.RecognizeAsync();
        }
    }
}