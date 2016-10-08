using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace voivode
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public const string StrategyHost = "http://st.wodserial.ru/";
        public const string AuthenticatePatterm = "username={0}&password={1}";
        public const string StrategyLink = "strategy/pp/pp.php";
        public const string WhereAreYouLink = "http://st.wodserial.ru/strategy/pp/pp.php?p=15";
        //public const string MineFiguresLink = "http://st.wodserial.ru/strategy/pp/pp.php?p=13";

        private readonly Web _browser = new Web { Agent = UserAgent.Firefox };
        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly Model _model = new Model();
        private readonly MapInfo _map = MapInfo.Instance;

        private void MainForm_Load(object sender, EventArgs e)
        {
            CreateMaps();
            if (!Login())
                Close();
            else
            {
                LoadModel();
            }
        }

        private void CreateMaps()
        {
            foreach (string city in _map.cities.Keys)
            {
                ToolStripButton button = new ToolStripButton();
                button.DisplayStyle = ToolStripItemDisplayStyle.Text;
                button.Name = city;
                button.Text = city;
                button.Click += toolStripButton_Click;
                toolStrip.Items.Add(button);
            }
            //pictureBox.SizeMode = 
        }

        private bool Login()
        {
            _browser.Get(StrategyHost, _cookies);
            using (PasswordDialog dialog = new PasswordDialog())
            {
                dialog.Username = Settings.Default.username;
                dialog.Password = Settings.Default.password;
                dialog.Remember = !string.IsNullOrEmpty(dialog.Username);
                while (true)
                {
                    DialogResult answer = dialog.ShowDialog();
                    if (answer != DialogResult.OK)
                        return false;
                    string authenticate = string.Format(AuthenticatePatterm, dialog.Username, dialog.Password);
                    string response = _browser.Post(StrategyHost, authenticate, StrategyHost, _cookies);
                    if (response.Contains(StrategyLink))
                    {
                        if (dialog.Remember)
                        {
                            Settings.Default.username = dialog.Username;
                            Settings.Default.password = dialog.Password;
                        }
                        else
                            Settings.Default.Reset();
                        Settings.Default.Save();
                        return true;
                    }
                }
            }
        }

        private void LoadModel()
        {
            string response = _browser.Get(WhereAreYouLink, StrategyHost, _cookies);
            _model.Load(response);
            toolStripStatusLabel.Text = _model.Title;
        }

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            foreach (ToolStripItem item in ((ToolStripButton)sender).GetCurrentParent().Items)
            {
                ToolStripButton b = item as ToolStripButton;
                if (b == null)
                    continue;
                b.Checked = (item == sender);
            }
            ToolStripButton button = (ToolStripButton) sender;
            string cityName = button.Name;
            CityInfo city = _map.cities[cityName];
            Bitmap pic = new Bitmap(city.image);
            using (Graphics g = Graphics.FromImage(pic))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                foreach (var pair in city.regions)
                {
                    string region = pair.Key;
                    Rectangle rect = pair.Value;
                    g.DrawRectangle(Pens.GreenYellow, rect);
                    g.DrawString(region, Font, Brushes.GreenYellow, rect);
                }
                g.Flush();
            }
            pictureBox.Image = pic;
        }

        private void toolStripComboBoxZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBoxZoom.SelectedIndex)
            {
                case 0:
                    pictureBox.Dock = DockStyle.None;
                    pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                    break;
                case 1:
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Dock = DockStyle.Fill;
                    break;
            }
        }
    }
}
