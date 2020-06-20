using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ytstorrent;

namespace ytsmovies
{
    class TorrentListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ObservableCollection<TorrentInfoModel> _torrentInfoList;
        public ObservableCollection<TorrentInfoModel> torrentInfoList
        {
            get
            {
                return _torrentInfoList;
            }
            set 
            { 
                _torrentInfoList = value;
                OnPropertyChanged(); 
            } 
        }

        public TorrentListViewModel()
        {
            torrentInfoList = new ObservableCollection<TorrentInfoModel>();
        }
    }
}
