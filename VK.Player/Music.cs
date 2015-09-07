using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace VK.Player
{
    public partial class Music : Form
    {
        public List<Audio> audioList;
        WMPLib.IWMPPlaylist PlayList;

        public Music()
        {
            InitializeComponent();
        }

        private void Music_Load(object sender, EventArgs e)
        {
            new Authorization().Show();
            backgroundWorker1.RunWorkerAsync();
        }

        public class Audio
        {
            public int aid { get; set; }
            public int owner_id { get; set; }
            public string artist { get; set; }
            public string title {get;set;}
            public int duration { get; set; }
            public string url { get; set; }
            public string lyrics_id { get; set; }
            public int genre { get; set; }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!App.Default.auth)
            {
                Thread.Sleep(500);
            }
            
            WebRequest req = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id=" + App.Default.id + "&need_user=0&access_token=" + App.Default.token);
            WebResponse res = req.GetResponse();
            var dataStream = res.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var resString = reader.ReadToEnd();
            reader.Close();
            res.Close();
            resString = HttpUtility.HtmlDecode(resString);

            var token = JToken.Parse(resString);
            audioList = token["response"].Children().Skip(1).Select(x => x.ToObject<Audio>()).ToList();

            this.Invoke((MethodInvoker)delegate
            {
                PlayList = axWindowsMediaPlayer1.playlistCollection.newPlaylist("vkPlayList");

                foreach (var item in audioList)
                {
                    PlayList.appendItem(axWindowsMediaPlayer1.newMedia(item.url));
                    listBox1.Items.Add(item.artist + " - " + item.title);
                }
                axWindowsMediaPlayer1.currentPlaylist = PlayList;
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            });
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                axWindowsMediaPlayer1.Ctlcontrols.currentItem = axWindowsMediaPlayer1.currentPlaylist.get_Item(listBox1.SelectedIndex);
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!App.Default.auth && audioList == null)
            {
                Thread.Sleep(500);
            }

            WebClient c = new WebClient();
            if (audioList != null)
            {
                int j = 0;
                for (int i = 11; i < audioList.Count; i++)
                {
                    if (j > 10)
                    {
                        j = 0;
                        Thread.Sleep(10000);
                    }
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.Text = "VK.Player - Downloading (" + i + "/" + audioList.Count + ")";
                    });
                    j++;
                    try
                    {
                        c.DownloadFile(audioList[i].url, "D:\\MusicVK\\" + audioList[i].artist + " - " + audioList[i].title + ".mp3");
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    Thread.Sleep(5000);
                }
                this.Invoke((MethodInvoker)delegate
                {
                    this.Text = "VK.Player";
                }); 
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker2.RunWorkerAsync();
        }
    }
}
