using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamFormsPocketSphinx
{
    public class MainPageViewModel
    {
        public bool IsListening { get; set; }

        public bool IsInSpeech { get; set; }

        public string Hypothesis { get; set; }

        public MainPageViewModel()
        {
            StartListening = new Command(OnStartListening);
            StopListening = new Command(OnStopListening);

            var speech = DependencyService.Get<ISpeechService>(DependencyFetchTarget.GlobalInstance);
            speech.Setup();
        }

        public Command StopListening { get; set; }

        public Command StartListening { get; set; }

        private void OnStopListening(object obj)
        {
            var speech = DependencyService.Get<ISpeechService>(DependencyFetchTarget.GlobalInstance);
            speech.StartListening();
        }

        private void OnStartListening(object obj)
        {
            var speech = DependencyService.Get<ISpeechService>(DependencyFetchTarget.GlobalInstance);
            speech.StartListening();
        }
    }
}
