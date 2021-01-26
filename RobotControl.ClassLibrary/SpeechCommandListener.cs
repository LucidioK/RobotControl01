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
        Grammar grammar;
        string latestText;
        object latestTextLock = new object();
        bool fresh = false;
        string[] commands = new string[] {
            "robot stop",
            "robot continue",
            "robot start",
            "robot go home",
            "robot configure",
        };


        public SpeechCommandListener(bool fake, IState state)
        {
            this.fake = fake;
            this.state = state;
            //foreach (var command in commands) grammarBuilder.Append(command);
            grammar = new Grammar("SpeechGrammar.xml");
            speechRecognitionEngine.SetInputToDefaultAudioDevice();
            //speechRecognitionEngine.LoadGrammar(/*new Grammar(grammarBuilder)*/dictationGrammar);
            speechRecognitionEngine.LoadGrammar(grammar);
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

        public string[] Commands => commands;

        private readonly bool fake;
        IState state;
        private PubSub pubSub = new PubSub();
        public void Subscribe(IPublishTarget publisherTarget) => pubSub.Subscribe(publisherTarget);

        private void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Result != null && Monitor.TryEnter(latestTextLock))
            {
                System.Diagnostics.Debug.WriteLine($"-->SpeechCommandListener.RecognizeCompleted: {string.Join(", ", e.Result.Alternates.Select(a => a.Text))}");
                foreach (var alternative in e.Result.Alternates)
                {
                    var s = alternative.Text.ToLowerInvariant().Trim();
                    if (commands.Contains(s))
                    {
                        System.Diagnostics.Debug.WriteLine($"-->SpeechCommandListener.RecognizeCompleted: Publishing {s}");
                        latestText = s;
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