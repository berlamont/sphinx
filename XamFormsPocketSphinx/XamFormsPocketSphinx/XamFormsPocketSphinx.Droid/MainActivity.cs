using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using SphinxBase;
using PocketSphinx;
using Android.Content.Res;
using System.Collections.Generic;
using Xamarin.Forms;
using XamarinAndroidSphinx;
using Java.IO;
using System.Threading.Tasks;

namespace XamFormsPocketSphinx.Droid
{
    [Activity(Label = "XamFormsPocketSphinx", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private SpeechRecognizer recognizer;

        private static string KWS_SEARCH = "wakeup";
        private static string FORECAST_SEARCH = "forecast";
        private static string DIGITS_SEARCH = "digits";
        private static string PHONE_SEARCH = "phones";
        private static string MENU_SEARCH = "menu";

        private static String KEYPHRASE = "oh mighty computer";

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);


            RunRecognizerSetup();



            //Java.IO.File file = Forms.Context.GetExternalFilesDir(null);



            //Config c = Decoder.DefaultConfig();
            //c.SetString("-hmm", "/storage/emulated/0/SPHINX/model/hmm/en-us");
            //c.SetString("-lm", "/storage/emulated/0/SPHINX/model/lm/en-us.lm.bin");
            //c.SetString("-dict", "/storage/emulated/0/SPHINX/model/dict/cmudict-en-us.dict");
            //Decoder d = new Decoder(c);

            //File.Exists("/storage/emulated/0/SPHINX/recording/goforward.raw");

            //bool create, erase;
            //var dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/SPHINX"); //  /storage/emulated/0/SPHINX
            //if (!dir.Exists())
            //    create = dir.Mkdirs();
            //if (dir.Exists())
            //    erase = dir.Delete();

            //byte[] data = null;

            //AssetManager assets = this.Assets;
            //using (var stream = assets.Open("recording/goforward.raw"))
            //{
            //    var memoryStream = new System.IO.MemoryStream();
            //    stream.CopyTo(memoryStream);
            //    data = memoryStream.ToArray();
            //    memoryStream.Close();
            //    assets.Close();
            //}

            //int n;
            //string[] list = new string[];
            //list = assets.List("model/hmm/cmudict-en-us.dict");
            //using (var stream = assets.Open("model/hmm/cmudict-en-us.dict"))
            //{
            //    var memoryStream = new MemoryStream();
            //    stream.CopyTo(memoryStream);
            //    data = memoryStream.ToArray();
            //    WriteFile("cmudict-en-us.dict", data);
            //    memoryStream.Close();
            //    assets.Close();
            //}

            //using (var stream = assets.Open("model/dict/cmudict-en-us.dict"))
            //{
            //    var memoryStream = new MemoryStream();
            //    stream.CopyTo(memoryStream);
            //    data = memoryStream.ToArray();
            //    WriteFile("cmudict-en-us.dict", data);
            //    memoryStream.Close();
            //    assets.Close();
            //}

            //using (var stream = assets.Open("model/dict/cmudict-en-us.dict"))
            //{
            //    var memoryStream = new MemoryStream();
            //    stream.CopyTo(memoryStream);
            //    data = memoryStream.ToArray();
            //    WriteFile("cmudict-en-us.dict", data);
            //    memoryStream.Close();
            //    assets.Close();
            //}

            // byte[] data = File.ReadAllBytes("/storage/emulated/0/SPHINX/recording/goforward.raw");
            //d.StartUtt();
            //d.ProcessRaw(data, data.Length, false, false);
            //d.EndUtt();

            //System.Console.WriteLine("Result is '{0}'", d.Hyp().Hypstr);

            //foreach (Segment s in d.Seg())
            //{
            //    System.Console.WriteLine(s);
            //}

            LoadApplication(new App());
        }

        private async Task RunRecognizerSetup()
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

            recognizer = new SpeechRecognizerSetup(config)
                .SetAcousticModel(new File(assetsDir, "en-us-ptm"))
                .SetDictionary(new File(assetsDir, "cmudict-en-us.dict"))
                //.SetRawLogDir(assetsDir) // To disable logging of raw audio comment out this call (takes a lot of space on the device)
                .GetRecognizer();

            recognizer.Result += Recognizer_Result;
            recognizer.InSpeechChange += Recognizer_InSpeechChange;
            recognizer.Timeout += Recognizer_Timeout;

            /** In your application you might not need to add all those searches.
             * They are added here for demonstration. You can leave just one.
             */

            // Create keyword-activation search.
            recognizer.AddKeyphraseSearch(KWS_SEARCH, KEYPHRASE);

            // Create grammar-based search for selection between demos
            File menuGrammar = new File(assetsDir, "menu.gram");
            recognizer.AddGrammarSearch(MENU_SEARCH, menuGrammar);

            // Create grammar-based search for digit recognition
            File digitsGrammar = new File(assetsDir, "digits.gram");
            recognizer.AddGrammarSearch(DIGITS_SEARCH, digitsGrammar);

            // Create language model search
            File languageModel = new File(assetsDir, "weather.dmp");
            recognizer.AddNgramSearch(FORECAST_SEARCH, languageModel);

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
            recognizer.Stop();

            // If we are not spotting, start listening with timeout (10000 ms or 10 seconds).
            if (searchName.Equals(KWS_SEARCH))
                recognizer.StartListening(searchName);
            else
                recognizer.StartListening(searchName, 10000);

            //String caption = getResources().getString(captions.get(searchName));
            //((TextView)findViewById(R.id.caption_text)).setText(caption);
        }

    }
}

