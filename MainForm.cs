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
		private Font textFont;
		private Font figureFont;

        private void MainForm_Load(object sender, EventArgs e)
        {
			textFont = new Font (Font.FontFamily, 1.8F*Font.Size);
			figureFont = new  Font (Font.FontFamily, 4*Font.Size);
			CreateMaps();
            if (!Login())
                Close();
            else
                LoadModel();
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
			ShowCity (cityName);
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

		private void ShowCity(string name)
		{
			CityInfo city = _map.cities[name];
			Bitmap pic = new Bitmap(city.image);
			using (Graphics g = Graphics.FromImage(pic))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				StringFormat sf = new StringFormat {
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Center,
				};
				SizeF sz = g.MeasureString ("W", figureFont);
				foreach (var pair in city.regions)
				{
					string region = pair.Key;
					Rectangle rect = pair.Value;
					List<Figure> figures;
					if (!_model.Regions.TryGetValue (region, out figures))
						continue;
					PointF origin = rect.Location;
					foreach (Figure f in figures)
					{
						origin.X += sz.Width * 0.1F;
						RectangleF labelRect = new RectangleF {
							Location = origin,
							Size = sz,
						};
						if (!string.IsNullOrEmpty (f.Thing))
						{
							Brush thingBrush;
							switch (f.Thing)
							{
							case "Ресурсы":
								thingBrush = Brushes.LightGreen;
								break;
							case "Союзники":
								thingBrush = Brushes.Crimson;
								break;
							case "Связи":
								thingBrush = Brushes.Blue;
								break;
							case "Тайны":
								thingBrush = Brushes.White;
								break;
							case "Подвиг":
								thingBrush = Brushes.DarkGray;
								break;
							default:
								thingBrush = Brushes.Fuchsia;
								break;
							}
							g.FillEllipse (thingBrush, labelRect);
							g.DrawEllipse (Pens.Black, labelRect);
						}
						// Ресурсы (зеленые)
						// Союзники (красные)
						// Связи (синие)
						// Тайны (белые) Тайны(1шт.) "№9: Петр и Павел"
						// Подвиг (золото/бронза/чёрные) 

						if (!string.IsNullOrEmpty (f.Piece)) {
							string piece;
							switch (f.Piece) {
							case "пешка":
								piece = "♟";
								break;
							case "конь":
								piece = "♞";
								break;
							case "офицер":
								piece = "♝";
								break;
							case "ладья":
								piece = "♜";
								break;
							case "ферзь":
								piece = "♛";
								break;
							case "король":
								piece = "♚";
								break;
							default:
								piece = "⚠";
								break;	
							}
							Color c = pic.GetPixel ((int)(origin.X + sz.Width / 2), (int)(origin.Y + sz.Width / 2));
							if (f.IsDown)
								sf.FormatFlags = StringFormatFlags.DirectionVertical;
							g.DrawString (piece, figureFont,
								c.GetBrightness () < 0.8 ? Brushes.Yellow : Brushes.Chocolate, labelRect, sf);
							sf.FormatFlags = 0;
						}
						if (!string.IsNullOrEmpty (f.Number))
						{
							labelRect.Y += sz.Height;
							labelRect.Height *= 0.5F;
							g.FillRectangle (Brushes.White, labelRect);
							g.DrawRectangle (Pens.Black, (int) labelRect.Left, (int) labelRect.Top,
								(int) labelRect.Right, (int) labelRect.Bottom);
							g.DrawString (f.Number, textFont, Brushes.Black, labelRect, sf);
						}
						origin.X += sz.Width;
						if (origin.X + sz.Width >= rect.Right)
						{
							origin.X = rect.Left;
							origin.Y += sz.Height*1.5F;
						}
					}
				}
				g.Flush();
			}
			pictureBox.Image = pic;

		}

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
			if (pictureBox.Image == null)
				return;
			using (SaveFileDialog dialog = new SaveFileDialog
				{
					AddExtension = true,
					CheckPathExists = true,
					DefaultExt = "jpg",
					OverwritePrompt = true,
					ValidateNames = true,
				})
			{
				if (dialog.ShowDialog () != DialogResult.OK)
					return;
				pictureBox.Image.Save (dialog.FileName);
			}
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
