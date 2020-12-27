using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.Net
{
    class Speaker : IPublishTarget
    {
        // Initialize a new instance of the SpeechSynthesizer.
        SpeechSynthesizer synth = new SpeechSynthesizer();

        public Speaker() => synth.SetOutputToDefaultAudioDevice();


        public void OnEvent(IEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.Name == EventName.PleaseSay)
            {
                synth.Speak(eventDescriptor.Detail);
            }
        }
    }
}
