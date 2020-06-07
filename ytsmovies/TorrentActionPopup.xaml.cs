using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ytstorrent;

namespace ytsmovies
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TorrentActionPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public Int32 index { set; get; }
        public TorrentActionPopup(Int32 itemIndex)
        {
            InitializeComponent();
            if(itemIndex!=0)
            {
                index = itemIndex;
            }
            Console.WriteLine("item clicked = {0}", index);            
        }

        private void start_Clicked(object sender, EventArgs e)
        {

        }

        private void pause_Clicked(object sender, EventArgs e)
        {

        }

        private void stop_Clicked(object sender, EventArgs e)
        {

        }

        private void remove_Clicked(object sender, EventArgs e)
        {
                    
        }

        private void delete_Clicked(object sender, EventArgs e)
        {

        }
    }
}