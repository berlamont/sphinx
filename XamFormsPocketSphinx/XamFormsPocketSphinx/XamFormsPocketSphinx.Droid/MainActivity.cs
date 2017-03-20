using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using SphinxBase;
using PocketSphinx;
using System.IO;
using Android.Content.Res;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamFormsPocketSphinx.Droid
{
    [Activity(Label = "XamFormsPocketSphinx", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            Java.IO.File file = Forms.Context.GetExternalFilesDir(null);



            Config c = Decoder.DefaultConfig();
            c.SetString("-hmm", "/storage/emulated/0/SPHINX/model/hmm/en-us");
            c.SetString("-lm", "/storage/emulated/0/SPHINX/model/lm/en-us.lm.bin");
            c.SetString("-dict", "/storage/emulated/0/SPHINX/model/dict/cmudict-en-us.dict");
            Decoder d = new Decoder(c);

            //File.Exists("/storage/emulated/0/SPHINX/recording/goforward.raw");

            //bool create, erase;
            //var dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/SPHINX"); //  /storage/emulated/0/SPHINX
            //if (!dir.Exists())
            //    create = dir.Mkdirs();
            //if (dir.Exists())
            //    erase = dir.Delete();

            byte[] data = null;

            AssetManager assets = this.Assets;
            using (var stream = assets.Open("recording/goforward.raw"))
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                memoryStream.Close();
                assets.Close();
            }

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
            d.StartUtt();
            d.ProcessRaw(data, data.Length, false, false);
            d.EndUtt();

            Console.WriteLine("Result is '{0}'", d.Hyp().Hypstr);

            foreach (Segment s in d.Seg())
            {
                Console.WriteLine(s);
            }

            LoadApplication(new App());
        }

        public static void WriteFile(string fileName, byte[] bytes)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!path.EndsWith(@"/"))
                path += @"/";

            if (File.Exists(Path.Combine(path, fileName)))
                File.Delete(Path.Combine(path, fileName));

            using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(bytes, 0, (int)bytes.Length);
                fs.Close();
            }
        }
    }
}

