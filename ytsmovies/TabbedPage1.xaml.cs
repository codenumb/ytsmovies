using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using ytstorrent;
using MonoTorrent.Client;
using MonoTorrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Essentials;
using System.IO;
using Xamarin.Forms.Markup;
using Rg.Plugins.Popup.Services;

namespace ytsmovies
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage
    {
        public FileData file;
        public MyTorrent torrent;
        public INotificationManager notificationManager;

        public TabbedPage1()
        {
            InitializeComponent();
            torrent = new MyTorrent();
            torrent.initDownloadMan();
            ListViewDownloads.ItemsSource = MyTorrent.TorrentInfoList;
            /*for (int i = 0; i < 150; i++)
            {

                Debug.WriteLine("its a test");
            }
            initTorrentEngineAsync();
            torrent.TorrentStateChanged += this.ontorrentStateChanged;
            init download page
            bool run = true;
            var listDownload = new ListView();
            ObservableCollection<downloads> employees = new ObservableCollection<downloads>();
            listDownload.ItemsSource = employees;

            while (run)
            {
                foreach (TorrentManager manager in MyTorrent.torrents)
                {

                }
            }*/

        }

        public async void initTorrentEngineAsync()
        {
            await MyTorrent.setupEngine();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {

            file = await CrossFilePicker.Current.PickFile();
            if (file != null)
            {

                //string torrentFile =System.IO.Path.Combine(file.FilePath,file.FileName);
                string torrentFile = file.FilePath;
                string torrentFileName = file.FileName;
                Console.WriteLine("path:{0}, filename{0}", torrentFile,torrentFileName);
                bool exist = File.Exists(torrentFile);
                if (!exist)
                {
                    Console.WriteLine("file does  not exsist");
                }
                label0.Text = torrentFile;
                //testImg.Source = "EC_3.png";
                testImg.Source = ImageSource.FromFile(torrentFile);
                await torrent.addTorrent(torrentFile, torrentFileName);
            }

        }

        public void ontorrentStateChanged(object sender, TorrentStateChangedEventArgs args)
        {
            if (args.NewState == TorrentState.Seeding && args.OldState == TorrentState.Downloading)
            {
                //torrent completed. send notification.
                Console.WriteLine(args.TorrentManager.Torrent.Name + "downloaded!");
                return;
            }

        }

        private  async void buttonExit_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("YTS exiting!....");
            await torrent.Shutdown();
        }

        public interface INotificationManager
        {
            event EventHandler NotificationReceived;

            void Initialize();

            int ScheduleNotification(string title, string message);

            void ReceiveNotification(string title, string message);
        }

        private void ListViewDownloads_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            /*open  a page containing torrent information*/
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            TappedEventArgs tappedEventArgs = (TappedEventArgs)e;
            PopupNavigation.Instance.PushAsync(new TorrentActionPopup((Int32)tappedEventArgs.Parameter));
        }

        private void ListViewDownloads_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }

    public class Downloads
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
   public class Person
    {
        public string Name { get; set; }
        public string Age    { get; set; }
        public string Location { get; set; }
    }
}