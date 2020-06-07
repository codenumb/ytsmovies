using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MonoTorrent.Client;
using ytstorrent;

namespace ytsmovies
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //var t = new TabbedPage1();
            MainPage = new TabbedPage1();
            //MainPage = t;
            //var label = new Label();
            //t.thejas
            //TorrentManager torrentManager;
                
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            Console.WriteLine("app state:sleep");
        }

        protected override void OnResume()
        {
            Console.WriteLine("app state: resumed");
        }
    }
}
