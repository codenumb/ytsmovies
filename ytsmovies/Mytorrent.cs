using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

using System.Threading;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using SampleClient;
using Xamarin.Forms.Internals;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Timers;
using Xamarin.Essentials;
using ytsmovies;
using System.Globalization;
using System.ComponentModel;
using System.Data.Common;

namespace ytstorrent
{
    public class MyTorrent : INotifyPropertyChanged
    {
        public static int index { set; get; }
        public static string basepath = FileSystem.AppDataDirectory;// Environment.CurrentDirectory;
        public static string dhtNodeFile= Path.Combine(basepath,"DhtNodes");
        public static string downloadsPath= "/storage/emulated/0/Download/";
        public static string fastResumeFile=Path.Combine(basepath,"fastresume.data");
        public static string torrentsPath= Path.Combine(basepath, "torrentPath");
        public static string torrentsInfoPath = Path.Combine(basepath, "torrentInfoPath");
        public static ClientEngine engine;				// The engine used for downloading
        public static Top10Listener listener;           // This is a subclass of TraceListener which remembers the last 20 statements sent to it
        public static BEncodedDictionary fastResume = new BEncodedDictionary();
        public delegate void OnStateChangedEventHandler(object o, TorrentStateChangedEventArgs e);
        public event OnStateChangedEventHandler TorrentStateChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public static System.Timers.Timer monitorTimer = new System.Timers.Timer();

        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public static ObservableCollection<TorrentInfoModel> TorrentInfoList { get; set; }
        
        //public static ObservableCollection<TorrentInfoModel> TorrentInfoList = new ObservableCollection<TorrentInfoModel>();

        public enum TorrentAction
        {
            START = 1,
            STOP,
            PAUSE,
            DELETE,
            REMOVE
        }
        public MyTorrent()
        {
            setupEngine();//.Wait();
            listener = new Top10Listener(10);
            monitorTimer.Interval = 2000;
            monitorTimer.Elapsed += CheckDownloadProgress;
            TorrentInfoList = new ObservableCollection<TorrentInfoModel>();
            MessagingCenter.Subscribe<TorrentActionPopup, Int32>(this, "StartThisTorrent",
                (sender, args) =>
                {
                    Console.WriteLine("Message: StartThisTorrent= {0}", args);
                    if(args>=0)
                    {
                        DoTorrentAction(TorrentInfoList[args], TorrentAction.START);
                    }
                }
            );

            MessagingCenter.Subscribe<TorrentActionPopup, Int32>(this, "StopThisTorrent",
                (sender, args) =>
                 {
                    Console.WriteLine("Message: StopThisTorrent= {0}", args);
                    if (args >= 0)
                     {
                         DoTorrentAction(TorrentInfoList[args], TorrentAction.STOP);
                     }
                 }
            );
            MessagingCenter.Subscribe<TorrentActionPopup, Int32>(this, "PauseThisTorrent",
                (sender, args) =>
                {
                    Console.WriteLine("Message: PauseThisTorrent= {0}", args);
                    if (args >= 0)
                    {
                        DoTorrentAction(TorrentInfoList[args], TorrentAction.PAUSE);
                    }
                }
            );
            MessagingCenter.Subscribe<TorrentActionPopup, Int32>(this, "RemoveThisTorrent",
                (sender, args) =>
                {
                    Console.WriteLine("Message: RemoveThisTorrent= {0}", args);
                    if (args >= 0)
                    {
                        DoTorrentAction(TorrentInfoList[args], TorrentAction.REMOVE);
                    }
                }
            );
            MessagingCenter.Subscribe<TorrentActionPopup, Int32>(this, "DeleteThisTorrent",
                (sender, args) =>
                {
                    Console.WriteLine("Message: DeleteThisTorrent= {0}", args);
                    if (args >= 0)
                    {
                        DoTorrentAction(TorrentInfoList[args], TorrentAction.DELETE);
                    }
                }
            );
        }
        public static async Task setupEngine() //methode for init torrent engine.
        {
            Debug.WriteLine("torrent init start");
            int port = 8765;
            Console.WriteLine("setupEngine()");
            EngineSettings settings = new EngineSettings();
            Console.WriteLine("downloadpath={0}", downloadsPath);
            Console.WriteLine("fastresumefile path= {0}", fastResumeFile);
            Console.WriteLine("basepath = {0}", basepath);
            Console.WriteLine("torrentInfoPath = {0}", torrentsInfoPath);
            Console.WriteLine("torrentFilePath = {0}", torrentsPath);
            Console.WriteLine("dhtnodefile = {0}", dhtNodeFile);
            settings.SavePath = downloadsPath;
            settings.MaximumUploadSpeed = 200 * 1024;
            engine = new ClientEngine(settings);
            byte[] nodes = Array.Empty<byte>();
            try
            {
                if (File.Exists(dhtNodeFile))
                    nodes = File.ReadAllBytes(dhtNodeFile);
            }
            catch
            {
                Console.WriteLine("No existing dht nodes could be loaded");
            }
            DhtEngine dht = new DhtEngine(new IPEndPoint(IPAddress.Any, port));
            await engine.RegisterDhtAsync(dht);
            await engine.DhtEngine.StartAsync(nodes);

            //create save path if not exist.
            if (!Directory.Exists(engine.Settings.SavePath))
                Directory.CreateDirectory(engine.Settings.SavePath);

            //create .torrent path if does not exsist.
            Debug.WriteLine("torren tpath:"+ torrentsPath);
            if (!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);
            if (!Directory.Exists(torrentsInfoPath))
                Directory.CreateDirectory(torrentsInfoPath);
            Debug.WriteLine("{0}:exist", torrentsPath);            
        }

        //method to add torrent to torrent engine. source can be magnetic uri or .torrent file
        public async Task addTorrent(string torrentFilePath,string torrentFileName)
        {
            Debug.WriteLine("adding:{0}",torrentFilePath);
            TorrentInfoModel torinfo = new TorrentInfoModel();

            Torrent torrent = null;
            if (String.IsNullOrEmpty(torrentFilePath))
            {
                Console.WriteLine("empty file path!");
                return;
            }
            if (torrentFilePath.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    torrent = await Torrent.LoadAsync(torrentFilePath);
                    Console.WriteLine("info hash = {0}",torrent.InfoHash.ToString());
                }
                catch (Exception e)
                {
                    Console.Write("Couldnot decode {0}: ", torrentFilePath);
                    Console.WriteLine(e.Message);
                    return;
                }
                TorrentManager manager = new TorrentManager(torrent, downloadsPath, new TorrentSettings());
                if (fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                    manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.InfoHash.ToHex()]));
                await engine.Register(manager);

                //add to torrent global list.
                Debug.WriteLine("added to list:{0}", torrentFilePath);
                manager.PeersFound += Manager_PeersFound;
               

                //save .torrent to torrent path to restore.
                string destPath = Path.Combine(torrentsPath, torrentFileName);
                destPath.Replace(' ', '_');
                torinfo.manager = manager;
                torinfo.torrentFileName = torrentFileName;// destPath;
                torinfo.status = "downlaoding";
                torinfo.imageUrl = "none";
                torinfo.dateAdded = DateTime.Now.ToString();
                writeTorrentInfoToFile(torinfo);
                System.IO.File.Copy(torrentFilePath, destPath,true);
                TorrentInfoList.Add(torinfo);
                initTorrentEvents(torinfo);
                DoTorrentAction(torinfo, TorrentAction.START);
                Console.WriteLine("Torrent Size={0}", manager.Torrent.Size);
                Console.WriteLine("Torrent Name= {0}", torinfo.manager.Torrent.Name);
                if (!monitorTimer.Enabled)//start monitor if not started already.
                {
                    monitorTimer.Enabled = true;
                }
                Debug.WriteLine("started:{0}", torrentFilePath);
            }
            return;
        }

        //public void saveTorrentFile()

        //hook events of each torrents to handlers.
        public static void initTorrentEvents(TorrentInfoModel torInfo)
        {
            torInfo.manager.PeerConnected += (o, e) => {
                lock (listener)
                    listener.WriteLine($"Connection succeeded: {e.Peer.Uri}");
                Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
            };
            torInfo.manager.ConnectionAttemptFailed += (o, e) => {
                lock (listener)
                    listener.WriteLine(
                        $"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer.AllowedEncryption}");
                Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer.AllowedEncryption}");
            };
            // Every time a piece is hashed, this is fired.
            torInfo.manager.PieceHashed += delegate (object o, PieceHashedEventArgs e) {
                lock (listener)
                    listener.WriteLine($"Piece Hashed: {e.PieceIndex} - {(e.HashPassed ? "Pass" : "Fail")}");
                    Console.WriteLine($"Piece Hashed: {e.PieceIndex} - {(e.HashPassed ? "Pass" : "Fail")}");
            };

            // Every time the state changes (Stopped -> Seeding -> Downloading -> Hashing) this is fired
            torInfo.manager.TorrentStateChanged += delegate (object o, TorrentStateChangedEventArgs e) {
                lock (listener)
                    listener.WriteLine($"OldState: {e.OldState} NewState: {e.NewState}");
                    Console.WriteLine($"OldState: {e.OldState} NewState: {e.NewState}");
                    TorrentInfoList[TorrentInfoList.IndexOf(torInfo)].status = e.NewState.ToString();
            };

            torInfo.manager.TorrentStateChanged += onStateChanged;
            // Every time the tracker's state changes, this is fired
            torInfo.manager.TrackerManager.AnnounceComplete += (sender, e) => {
                listener.WriteLine($"{e.Successful}: {e.Tracker}");
                Console.WriteLine($"{e.Successful}: {e.Tracker}");
            };

        }

        //start individual torrents.
        public async void DoTorrentAction(TorrentInfoModel torinfo, TorrentAction state)
        {
            string filePath;
            bool response=false;
            switch (state)
            {
                case TorrentAction.START:
                    await torinfo.manager.StartAsync();
                    TorrentInfoList[TorrentInfoList.IndexOf(torinfo)].status = "Downloading";
                    writeTorrentInfoToFile(TorrentInfoList[TorrentInfoList.IndexOf(torinfo)]);
                    break;
                case TorrentAction.STOP:
                    await torinfo.manager.StopAsync();
                    TorrentInfoList[TorrentInfoList.IndexOf(torinfo)].status = "Stopped";
                    writeTorrentInfoToFile(TorrentInfoList[TorrentInfoList.IndexOf(torinfo)]);
                    break;
                case TorrentAction.PAUSE:
                    await torinfo.manager.PauseAsync();
                    TorrentInfoList[TorrentInfoList.IndexOf(torinfo)].status = "Paused";
                    writeTorrentInfoToFile(TorrentInfoList[TorrentInfoList.IndexOf(torinfo)]);
                    break;
                case TorrentAction.DELETE:
                    /*delete .torrent file, infoFile, downloaded files*/
                    response= await App.Current.MainPage.DisplayAlert("Delete?", "Would you like to delete your data?", "Yes", "No");
                    if (!response)
                        break;
                    filePath = Path.Combine(torrentsPath, torinfo.torrentFileName);
                    //TorrentInfoList.RemoveAt(torinfo.index - 1);
                    Console.WriteLine("deleting {0}", filePath);
                    deleteFile(filePath, false);
                    filePath = Path.Combine(downloadsPath, torinfo.manager.Torrent.Name);
                    Console.WriteLine("deleting {0}", filePath);
                    deleteFile(filePath, true);
                    Console.WriteLine("deleting {0}", torinfo.torrentInfoFileName);
                    deleteFile(torinfo.torrentInfoFileName, false);
                    await TorrentInfoList[TorrentInfoList.IndexOf(torinfo)].manager.StopAsync();
                    await engine.Unregister(torinfo.manager);
                    TorrentInfoList.Remove(torinfo);
                    break;
                case TorrentAction.REMOVE:
                    /*delete .torrent and info file*/
                    response = await App.Current.MainPage.DisplayAlert("Remove?", "Would you like to remove your data?", "Yes", "No");
                    if (!response)
                        break;
                    filePath = Path.Combine(torrentsPath, torinfo.torrentFileName);
                    //TorrentInfoList.RemoveAt(torinfo.index - 1);
                    Console.WriteLine("deleting {0}", filePath);
                    deleteFile(filePath, false);
                    Console.WriteLine("deleting {0}", torinfo.torrentInfoFileName);
                    deleteFile(torinfo.torrentInfoFileName, false);
                    await TorrentInfoList[TorrentInfoList.IndexOf(torinfo)].manager.StopAsync();
                    await engine.Unregister(torinfo.manager);
                    TorrentInfoList.Remove(torinfo);
                    break;
                default:
                    break;

                    //case Torrentstate.RESUME:
                    //  await manager.();
                    //break;
            }

        }
        public void deleteFile(string path, bool Directory)
        {
            if(Directory)
            {
                try
                {
                    System.IO.Directory.Delete(path, true);
                    Console.WriteLine("{0} Deleted", path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't delete {0}: {1}", path, e.Message);
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        Console.WriteLine("{0} Deleted", path);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't delete {0}: {1}", path, e.Message);
                    }
                }
            }

        }
        EncryptionTypes ChooseEncryption()
        {
            EncryptionTypes encryption;
            encryption = EncryptionTypes.All;
            encryption = EncryptionTypes.PlainText | EncryptionTypes.RC4Full | EncryptionTypes.RC4Header;
            return encryption;
        }

        //handler when a pear is added. 
        static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
        {
            lock (listener)
                listener.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");//throw new Exception("The method or operation is not implemented.");
        }

        public void CheckDownloadProgress(object o, System.Timers.ElapsedEventArgs e)
        {
            int i;
            for (i = 0; i < TorrentInfoList.Count; i++)
            {

                Console.WriteLine("{0}= {1} speeed= {2} Kb/s state={3}", TorrentInfoList[i].manager.Torrent.Name, TorrentInfoList[i].manager.Progress.ToString(),(TorrentInfoList[i].manager.Monitor.DownloadSpeed/1024.0), TorrentInfoList[i].manager.State);
                System.Threading.Thread.Sleep(1000);
                if (TorrentInfoList[i].manager.Progress == 100.0)
                {
                    Console.WriteLine("{0} downloaded!", TorrentInfoList[i].manager.Torrent.Name);
                    return;
                }
                monitorTimer.Enabled = true;
            }
        }
        public async Task Shutdown()
        {
            BEncodedDictionary fastResume = new BEncodedDictionary();
            for (int i = 0; i < TorrentInfoList.Count; i++)
            {
                var stoppingTask = TorrentInfoList[i].manager.StopAsync();
                while (TorrentInfoList[i].manager.State != TorrentState.Stopped)
                {
                    Console.WriteLine("{0} is {1}", TorrentInfoList[i].manager.Torrent.Name, TorrentInfoList[i].manager.State);
                    Thread.Sleep(250);
                }
                await stoppingTask;

                if (TorrentInfoList[i].manager.HashChecked)
                    fastResume.Add(TorrentInfoList[i].manager.Torrent.InfoHash.ToHex(), TorrentInfoList[i].manager.SaveFastResume().Encode());
            }

            var nodes = await engine.DhtEngine.SaveNodesAsync();
            File.WriteAllBytes(dhtNodeFile, nodes);
            File.WriteAllBytes(fastResumeFile, fastResume.Encode());
            engine.Dispose();
            Thread.Sleep(200);
            System.Environment.Exit(0);

        }
        public static void onStateChanged(object o, TorrentStateChangedEventArgs e)
        {
            //publish event
            Console.WriteLine("event trigger:" + e.TorrentManager.Torrent.Name);
        }

        public virtual void myTorrentStateChanged(object o, TorrentStateChangedEventArgs e)
        {
            TorrentStateChanged(o, e);
        }

        public async Task initDownloadMan()
        {
            string torFilePath;
            Torrent torrent = null;
            bool tmp = false;

            /*Load fast resume file*/
            try
            {
                if (File.Exists(fastResumeFile))
                    fastResume = BEncodedValue.Decode<BEncodedDictionary>(File.ReadAllBytes(fastResumeFile));
            }
            catch
            {
                Debug.WriteLine("error here");
            }
            /*open torrentfile dir and load all the torrents*/
            if (System.IO.Directory.Exists(torrentsInfoPath))
            {
                string[] movies = System.IO.Directory.GetFiles(torrentsInfoPath);
                foreach (string m in movies)
                {
                    TorrentInfoModel torInfo = new TorrentInfoModel();
                    readTorrentInfoFromFile(torInfo, m);
                    torFilePath = Path.Combine(torrentsPath, torInfo.torrentFileName);
                    if (System.IO.File.Exists(torFilePath))
                    {
                        Console.WriteLine("adding f= {0}", torFilePath);
                        torrent = await Torrent.LoadAsync(torFilePath);
                        TorrentManager manager = new TorrentManager(torrent, downloadsPath, new TorrentSettings());
                        manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.InfoHash.ToHex()]));
                        await engine.Register(manager);
                        torInfo.manager = manager;
                        torInfo.torrentFileName = torFilePath;
                        TorrentInfoList.Add(torInfo);
                        if(torInfo.status == TorrentState.Downloading.ToString())
                        {
                            await manager.StartAsync();
                        }
                        initTorrentEvents(torInfo); // hook all the events
                        tmp = true;
                    }
                }
                if (tmp)
                    monitorTimer.Enabled = true;
                Debug.WriteLine("torrent init exit!");
                //await engine.StartAllAsync();
            }
        }

        public void writeTorrentInfoToFile(TorrentInfoModel torInfo)
        {
            BinaryWriter bw=null;
            string fpath = Path.Combine(torrentsInfoPath, torInfo.manager.Torrent.Name.Replace(' ', '_'));
            Console.WriteLine("torrent-info-file-path={0}", fpath);
            try
            {
                bw = new BinaryWriter(new FileStream(fpath, FileMode.OpenOrCreate));
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message + "\n cant create file");
            }
            try
            {
                bw.Write(torInfo.torrentFileName);
                bw.Write(torInfo.status);
                bw.Write(torInfo.imageUrl);
                bw.Write(torInfo.dateAdded);
                //bw.Write(torInfo.genre);
                //bw.Write(torInfo.language);
                //bw.Write(torInfo.rating);
                //bw.Write(torInfo.resolution);
                //bw.Write(torInfo.year);
                //bw.Write(torInfo.duration);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message + "\n Cannot write to file.");
                return;
            }
            bw.Close();
        }
        public void readTorrentInfoFromFile(TorrentInfoModel torInfo, string fileName)
        {
            BinaryReader br=null;
            try
            {
                br = new BinaryReader(new FileStream(fileName, FileMode.Open));
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message + "\n cant create file");
            }
            try
            {
                torInfo.torrentFileName = br.ReadString();
                torInfo.status = br.ReadString();
                torInfo.imageUrl = br.ReadString();
                torInfo.torrentInfoFileName = fileName;
                torInfo.dateAdded = br.ReadString();
                //torInfo.genre = br.ReadString();
                //torInfo.language = br.ReadString();
                //torInfo.rating = br.ReadString();
                //torInfo.resolution = br.ReadString();
                //torInfo.year = br.ReadString();
                //torInfo.duration = br.ReadString();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message + "\n Cannot write to file.");
                return;
            }
            br.Close();
        }

    }

    public class TorrentInfoModel : INotifyPropertyChanged
    {
        public TorrentManager manager { get; set; }

        public string dateAdded { get; set; }

        private double _progress;
        public double progress 
        {
            get { return _progress; }
            set { _progress = value; OnPropertyChanged(); }
        }

        private string _speedDl;
        public string speedDl
        {
            get { return _speedDl; }
            set { _speedDl = value; OnPropertyChanged(); }
        }

        private string _speedUp;
        public string speedUp
        {
            get { return _speedUp; }
            set { _speedUp = value; OnPropertyChanged(); }
        }

        private string _status;
        public string status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        //private TorrentState _state;
        //public TorrentState state
        //{
        //    get { return _state; }
        //    set { _state = value; OnPropertyChanged(); }
        //}
        public string imageUrl { get; set; }
        public string genre { get; set; }
        public string year { get; set; }
        public string duration { get; set; }
        public string resolution { get; set; }
        public string rating { get; set; }
        public string language { get; set; }
        public string torrentFileName { get; set; }
        public string torrentInfoFileName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

