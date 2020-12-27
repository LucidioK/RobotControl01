using System;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;

namespace RobotControl.Net
{
    internal class SpeechCommandListener : ISubscriptionTarget
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


        public SpeechCommandListener(IState state)
        {
            this.state = state;
            foreach (var command in commands) grammarBuilder.Append(command);

            speechRecognitionEngine.SetInputToDefaultAudioDevice();
            speechRecognitionEngine.LoadGrammar(/*new Grammar(grammarBuilder)*/dictationGrammar);
            thread = new Thread(new ThreadStart(SpeechCommandListenerThread));
            thread.Start();
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
        IState state;
        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

        private void SpeechCommandListenerThread()
        {
            while (true)
            {
                var result = speechRecognitionEngine.Recognize();
                if (result != null && Monitor.TryEnter(latestTextLock))
                {
                    foreach (var alternative in result.Alternates)
                    {
                        if (commands.Contains(alternative.Text))
                        {
                            latestText = result.Text;
                            pubSub.Publish(new EventDescriptor { Name = EventName.VoiceCommandDetected, Detail = latestText });
                            fresh = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}