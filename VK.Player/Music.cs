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
                foreach (var item in audioList)
                {
                    listBox1.Items.Add(item.artist + " - " + item.title);
                }
            });
        }
    }
}
