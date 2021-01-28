using System.Speech.Synthesis;

namespace RobotControl.ClassLibrary
{
    class Speaker : RobotControlBase, ISpeaker
    {
        SpeechSynthesizer synth = new SpeechSynthesizer();

        public Speaker(IMediator mediator) : base(mediator) => synth.SetOutputToDefaultAudioDevice();

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
