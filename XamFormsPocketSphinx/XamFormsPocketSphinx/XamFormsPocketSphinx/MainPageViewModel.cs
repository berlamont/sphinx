using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamFormsPocketSphinx.Annotations;

namespace XamFormsPocketSphinx
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string _hypothesis;
        private bool _isInSpeech;
        private bool _isListening;

        public bool IsListening
        {
            get { return _isListening; }
            set
            {
                _isListening = value;
                OnPropertyChanged();
            }
        }

        public bool IsInSpeech
        {
            get { return _isInSpeech; }
            set
            {
                _isInSpeech = value;
                OnPropertyChanged();
            }
        }

        public string Hypothesis
        {
            get { return _hypothesis; }
            set
            {
                _hypothesis = value;
                OnPropertyChanged();
            }
        }

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
            speech.StopListening();
        }

        private void OnStartListening(object obj)
        {
            var speech = DependencyService.Get<ISpeechService>(DependencyFetchTarget.GlobalInstance);
            speech.StartListening();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
