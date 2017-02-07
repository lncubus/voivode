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

        public string StrategyHost = "http://st.wodserial.ru/";
        //public const string StrategyHost = "http://strateg.wodserial.ru/";
        public const string AuthenticatePatterm = "username={0}&password={1}";
        public const string StrategyLink = "strategy/pp/pp.php";
        public const string WhereAreYouLink = "strategy/pp/pp.php?p=15";
        //public const string MineFiguresLink = "http://st.wodserial.ru/strategy/pp/pp.php?p=13";

        private readonly Web _browser = new Web { Agent = UserAgent.Firefox };
        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly Model _model = new Model();
		private readonly MapInfo _map = new MapInfo("maps.ini");
		private readonly IDictionary<string, ToolStripButton> _buttons =
			new SortedDictionary<string, ToolStripButton>();
		private Font textFont;
		private Font figureFont;
		private Font pawnFont;

        private void MainForm_Load(object sender, EventArgs e)
        {
			textFont = new Font (Font.FontFamily, Font.Size);
			figureFont = new  Font (Font.FontFamily, 2.2F*Font.Size);
			pawnFont = new  Font (Font.FontFamily, 1.7F*Font.Size);
			CreateMaps();
            if (!Login())
                Close();
            else
            {
                LoadModel();
                ShowCityButtons();
                ClickFirstCityButton();
            }
        }

        private void CreateMaps()
        {
            foreach (string city in _map.cities.Keys)
            {
                ToolStripButton button = new ToolStripButton()
				{
	                DisplayStyle = ToolStripItemDisplayStyle.Text,
	                Name = city,
	                Text = city,
					Visible = false,
				};
				button.Click += toolStripButton_Click;
                toolStrip.Items.Add(button);
				_buttons.Add(city, button);
            }
            //pictureBox.SizeMode = 
        }

        private bool Login()
        {
            using (PasswordDialog dialog = new PasswordDialog())
            {
                char[] delims = null;
                dialog.Hosts = Settings.Default.hosts.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                dialog.Host = Settings.Default.host;
                dialog.Username = Settings.Default.username;
                dialog.Password = Settings.Default.password;
                dialog.Remember = !string.IsNullOrEmpty(dialog.Username);
                while (true)
                {
                    DialogResult answer = dialog.ShowDialog();
                    if (answer != DialogResult.OK)
                        return false;
                    StrategyHost = dialog.Host;
                    string authenticate = string.Format(AuthenticatePatterm, dialog.Username, dialog.Password);
                    _browser.Get(StrategyHost, _cookies);
                    string response = _browser.Post(StrategyHost, authenticate, StrategyHost, _cookies);
                    if (response.Contains(StrategyLink))
                    {
                        if (dialog.Remember)
                        {
                            Settings.Default.username = dialog.Username;
                            Settings.Default.password = dialog.Password;
                            Settings.Default.host = dialog.Host;
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
            string response = _browser.Get(StrategyHost + WhereAreYouLink, StrategyHost, _cookies);
            _model.Load(response);
            toolStripStatusLabel.Text = _model.Title;
            labelAlert.AutoSize = true;
            labelAlert.ForeColor = Color.Firebrick;
            labelAlert.Font = figureFont;
            labelAlert.Text = _model.Alert ?? string.Empty;
            labelAlert.Visible = !string.IsNullOrEmpty(_model.Alert);
        }

        private void ShowCityButtons()
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                ToolStripButton b = item as ToolStripButton;
                if (b == null || !_buttons.Values.Contains(b))
                    continue;
                string cityName = b.Name;
                CityInfo city = _map.cities[cityName];
                b.Visible = _model.Regions.Keys.Intersect(city.regions.Keys).Any();
            }
        }

        private void ClickFirstCityButton()
        {
            toolStripComboBoxZoom.SelectedIndex = 0;
            foreach (ToolStripItem item in toolStrip.Items)
            {
                ToolStripButton b = item as ToolStripButton;
                if (b == null || !_buttons.Values.Contains(b) || !b.Visible)
                    continue;
                toolStripButton_Click(b, null);
                break;
            }
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
            var image = pictureBox.Image;
            switch (toolStripComboBoxZoom.SelectedIndex)
            {
                case 0: // 100%
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
			    sz.Width = sz.Height;
				foreach (var pair in city.regions)
				{
					string region = pair.Key;
					Rectangle rect = pair.Value;
					List<Figure> figures;
					if (!_model.Regions.TryGetValue(region, out figures))
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
								piece = "?";
								break;	
							}
							Color c = pic.GetPixel ((int)(origin.X + sz.Width / 2), (int)(origin.Y + sz.Width / 2));
                            if (f.IsDown)
                                g.RotateTransform(90, MatrixOrder.Prepend);
                                //sf.FormatFlags = StringFormatFlags.DirectionVertical;
                            g.TranslateTransform(origin.X + sz.Width / 2, origin.Y + sz.Width / 2, MatrixOrder.Append);
                            g.DrawString (piece,
								piece != "♟" ? figureFont : pawnFont, 
								c.GetBrightness () < 0.8 ? Brushes.Yellow : Brushes.Chocolate, 0, 0, sf);
                            g.ResetTransform();
                            if (f.IsCaptured)
                            {
                                var bars = new[]
                                {
                                     new RectangleF { X = 0.25F, Y = 0F, Width = 0.05F, Height = 1F },
                                     //new RectangleF { X = 0.48F, Y = 0F, Width = 0.04F, Height = 1F },
                                     new RectangleF { X = 0.70F, Y = 0F, Width = 0.05F, Height = 1F },
                                     new RectangleF { X = 0F, Y = 0.25F, Width = 1F, Height = 0.05F },
                                     //new RectangleF { X = 0F, Y = 0.48F, Width = 1F, Height = 0.04F },
                                     new RectangleF { X = 0F, Y = 0.70F, Width = 1F, Height = 0.05F },
                                };
                                for(int b = 0; b < bars.Length; b++)
                                {
                                    var bar = bars[b];
                                    bars[b] = new RectangleF
                                    {
                                        X = labelRect.X + bar.X * labelRect.Width,
                                        Y = labelRect.Y + bar.Y * labelRect.Height,
                                        Width = bar.Width * labelRect.Width,
                                        Height = bar.Height * labelRect.Height,
                                    };
                                }
                                g.FillRectangles(Brushes.Red, bars);
                            }
                            //sf.FormatFlags = 0;
                        }
						if (!string.IsNullOrEmpty (f.Number))
						{
							labelRect.Y += sz.Height;
							labelRect.Height *= 0.5F;
							g.FillRectangle (Brushes.White, labelRect);
							g.DrawRectangle (Pens.Black, (int) labelRect.Left, (int) labelRect.Top,
								(int) labelRect.Width, (int) labelRect.Height);
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
					DefaultExt = "png",
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
