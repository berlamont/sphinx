using System;
using System.Threading.Tasks;
using Java.IO;
using PocketSphinx;
using SphinxBase;
using Xamarin.Forms;
using XamarinAndroidSphinx;
using XamFormsPocketSphinx.Droid;

[assembly: Dependency(typeof(SpeechService))]
namespace XamFormsPocketSphinx.Droid
{
    public class SpeechService: ISpeechService
    {
        private SpeechRecognizer _recognizer;

        private static string KWS_SEARCH = "wakeup";
        private static string FORECAST_SEARCH = "forecast";
        private static string DIGITS_SEARCH = "digits";
        private static string PHONE_SEARCH = "phones";
        private static string MENU_SEARCH = "menu";

        private static String KEYPHRASE = "oh mighty computer";

        public void StartListening()
        {
            switchSearch(KWS_SEARCH);

        }

        public void StopListening()
        {
            MainPage.ViewModel.IsListening = _recognizer.Stop();
        }


        public async Task Setup()
        {
            Assets assets = new Assets(Forms.Context);
            File assetDir = await assets.syncAssets();
            SetupRecognizer(assetDir);
        }

        private void SetupRecognizer(File assetsDir)
        {
            // The recognizer can be configured to perform multiple searches
            // of different kind and switch between them

            Config config = Decoder.DefaultConfig();

            _recognizer = new SpeechRecognizerSetup(config)
                .SetAcousticModel(new File(assetsDir, "en-us-ptm"))
                .SetDictionary(new File(assetsDir, "cmudict-en-us.dict"))
                //.SetRawLogDir(assetsDir) // To disable logging of raw audio comment out this call (takes a lot of space on the device)
                .GetRecognizer();

            _recognizer.Result += Recognizer_Result;
            _recognizer.InSpeechChange += Recognizer_InSpeechChange;
            _recognizer.Timeout += Recognizer_Timeout;

            /** In your application you might not need to add all those searches.
             * They are added here for demonstration. You can leave just one.
             */

            // Create keyword-activation search.
            _recognizer.AddKeyphraseSearch(KWS_SEARCH, KEYPHRASE);

            // Create grammar-based search for selection between demos
            File menuGrammar = new File(assetsDir, "menu.gram");
            _recognizer.AddGrammarSearch(MENU_SEARCH, menuGrammar);

            // Create grammar-based search for digit recognition
            File digitsGrammar = new File(assetsDir, "digits.gram");
            _recognizer.AddGrammarSearch(DIGITS_SEARCH, digitsGrammar);

            // Create language model search
            File languageModel = new File(assetsDir, "weather.dmp");
            _recognizer.AddNgramSearch(FORECAST_SEARCH, languageModel);

            // Phonetic search
            /*File phoneticModel = new File(assetsDir, "en-phone.dmp");
            recognizer.Ad(PHONE_SEARCH, phoneticModel);*/

            switchSearch(KWS_SEARCH);

        }

        private void Recognizer_Timeout(object sender, EventArgs e)
        {
            int i = 0;
        }

        private void Recognizer_InSpeechChange(object sender, bool e)
        {
            MainPage.ViewModel.IsInSpeech = e;
            int i = 0;
        }

        private void Recognizer_Result(object sender, SpeechResultEvent e)
        {
            bool isFinalResult = e.FinalResult;
            if (e.Hypothesis != null)
            {
                System.Diagnostics.Debug.WriteLine(e.Hypothesis.Hypstr);
            }
        }

        private void switchSearch(String searchName)
        {

            MainPage.ViewModel.IsListening = _recognizer.Stop();

            // If we are not spotting, start listening with timeout (10000 ms or 10 seconds).
            if (searchName.Equals(KWS_SEARCH))
                _recognizer.StartListening(searchName);
            else
                _recognizer.StartListening(searchName, 10000);


            MainPage.ViewModel.IsListening = true;

            //String caption = getResources().getString(captions.get(searchName));
            //((TextView)findViewById(R.id.caption_text)).setText(caption);
        }

    }
}