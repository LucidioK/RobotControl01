using System.Speech.Synthesis;

namespace RobotControl.Net
{
    class Speaker : ISpeaker
    {
        SpeechSynthesizer synth = new SpeechSynthesizer();

        public Speaker() => synth.SetOutputToDefaultAudioDevice();

        public EventName[] HandledEvents => new EventName[] { EventName.PleaseSay };

        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.Name == EventName.PleaseSay)
            {
                synth.Speak(eventDescriptor.Detail);
            }
        }
    }
}
