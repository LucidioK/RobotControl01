using System;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;

namespace RobotControl.ClassLibrary
{
    internal class SpeechCommandListener : RobotControlBase, ISpeechCommandListener
    {
        SpeechRecognitionEngine speechRecognitionEngine = new SpeechRecognitionEngine();

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


        public SpeechCommandListener(IMediator mediator, bool fake, IState state)
            : base(mediator)
        {
            this.fake = fake;
            this.state = state;

            if (!fake)
            {
                //foreach (var command in commands) grammarBuilder.Append(command);
                grammar = new Grammar("SpeechGrammar.xml");
                speechRecognitionEngine.SetInputToDefaultAudioDevice();
                //speechRecognitionEngine.LoadGrammar(/*new Grammar(grammarBuilder)*/dictationGrammar);
                speechRecognitionEngine.LoadGrammar(grammar);
                speechRecognitionEngine.RecognizeCompleted += RecognizeCompleted;
                speechRecognitionEngine.RecognizeAsync();
            }
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
                        Publish(new EventDescriptor { Name = EventName.VoiceCommandDetected, Detail = latestText });
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