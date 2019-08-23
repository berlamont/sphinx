using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamFormsPocketSphinx
{
    public partial class MainPage : ContentPage
    {
        public static MainPageViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = ViewModel = new MainPageViewModel();
        }
    }
}
