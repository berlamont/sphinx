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
      

        protected override async void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

    }
}

