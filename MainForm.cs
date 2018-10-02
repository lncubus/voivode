using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace voivode
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public string StrategyHost;
        public const string AuthenticatePattern = "username={0}&password={1}";
		public const string LoginContainer = "loginContainer";
        public const string WhereAreYouLink = "strategy/pp/pp.php?p=15";

        private readonly Web _browser = new Web { Agent = UserAgent.Chrome };
        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly Model _model = new Model();
		private MapInfo _map;
		private readonly IDictionary<string, ToolStripButton> _buttons =
			new SortedDictionary<string, ToolStripButton>();
		private Font textFont;
		private Font bigFont;

        private void MainForm_Load(object sender, EventArgs e)
        {
			textFont = new Font (Font.FontFamily, Font.Size);
			bigFont = new Font (Font.FontFamily, 2.0F*Font.Size);
			_map = new MapInfo("maps.ini");
			CreateMapButtons();
			if (!Login())
				this.BeginInvoke(new MethodInvoker(this.Close));
            else
            {
                LoadModel();
                ShowCityButtons();
                ClickFirstCityButton();
            }
        }

        private void CreateMapButtons()
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
                    string authenticate = string.Format(AuthenticatePattern, dialog.Username, dialog.Password);
                    _browser.Get(StrategyHost, _cookies);
                    string response = _browser.Post(StrategyHost, authenticate, StrategyHost, _cookies);
					if (response.Contains(LoginContainer))
					{
						string alert = Model.Alert(response);
                        dialog.Alert = alert;
					}
					else
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
            labelAlert.Font = bigFont;
			string alert = Model.Alert(response);
			labelAlert.Visible = !string.IsNullOrEmpty(alert);
			labelAlert.Text = alert ?? string.Empty;
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
			toolStripComboBoxZoom_SelectedIndexChanged(toolStripComboBoxZoom, null);
        }

        private void toolStripComboBoxZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            var image = pictureBox.Image;
            panelClient.HorizontalScroll.Value = 0;
            panelClient.VerticalScroll.Value = 0;
            panelClient.ScrollControlIntoView(pictureBox);
            switch (toolStripComboBoxZoom.Text)
            {
                case "100%": // 100%
                    pictureBox.Dock = DockStyle.None;
                    pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                    break;
				case "Fit":
					pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
					pictureBox.Dock = DockStyle.Fill;
					break;
				default:
					pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
					pictureBox.Dock = DockStyle.None;
					if (image == null)
						return;
					int scale = int.Parse(toolStripComboBoxZoom.Text.Replace('%', '0'));
					pictureBox.Height = (scale * image.Height) / 1000;
					pictureBox.Width = (scale * image.Width) / 1000;
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
				foreach (var pair in city.regions)
				{
					string region = pair.Key;
					Rectangle rect = pair.Value;
					List<Figure> figures;
					if (!_model.Regions.TryGetValue(region, out figures))
						continue;
                    SizeF sz = g.MeasureString("W", bigFont);
                    sz.Width = sz.Height;
                    PointF origin = rect.Location;
                    int w = (int)(rect.Width / sz.Width);
                    int h = (int)(rect.Height / sz.Height);
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
                            int x = (int)(origin.X + sz.Width / 2);
                            int y = (int)(origin.Y + sz.Width / 2);
                            if (x >= pic.Width)
                                x = pic.Width - 1;
                            if (y >= pic.Height)
                                y = pic.Height - 1;
                            Color c = pic.GetPixel (x, y);
                            if (f.IsDown || f.IsCaptured)
                                g.RotateTransform(90, MatrixOrder.Prepend);
                                //sf.FormatFlags = StringFormatFlags.DirectionVertical;
                            g.TranslateTransform(origin.X + sz.Width / 2, origin.Y + sz.Width / 2, MatrixOrder.Append);
                            g.DrawString (piece,
                                //piece != "♟" ? figureFont : pawnFont,
                                bigFont,
                                c.GetBrightness () < 0.7 ? Brushes.Yellow : Brushes.Brown, 0, 0, sf);
                            g.ResetTransform();
                            if (f.IsCaptured)
                            {
                                if (!string.IsNullOrEmpty(f.Captor))
                                {
                                    var tagSize = g.MeasureString(f.Captor, textFont);
                                    tagSize.Width *= 1.1F;
                                    tagSize.Height *= 1.1F;
                                    var captorRect = new RectangleF
                                    {
                                        X = origin.X,
                                        Y = origin.Y + sz.Height - tagSize.Height,
                                        Width = tagSize.Width,
                                        Height = tagSize.Height
                                    };
                                    //labelRect.Y += sz.Height;
                                    //labelRect.Height *= 0.5F;
                                    g.FillRectangle(Brushes.Yellow, captorRect);
                                    g.DrawRectangle(Pens.Black, (int)captorRect.Left, (int)captorRect.Top,
                                        (int)captorRect.Width, (int)captorRect.Height);
                                    g.DrawString(f.Captor, textFont, Brushes.Red, captorRect, sf);
                                }
                                else
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
                                    for (int b = 0; b < bars.Length; b++)
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
                            }
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
					DefaultExt = "jpeg",
                    Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp;  *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png",
                    OverwritePrompt = true,
					ValidateNames = true,
				})
			{
				if (dialog.ShowDialog () != DialogResult.OK)
					return;
				SavePicture(dialog.FileName, pictureBox.Image);
			}
        }

		public static void SavePicture(string filename, Image image)
		{
			ImageFormat f;
			string ext = Path.GetExtension(filename).Remove(0,1).ToLowerInvariant();
			switch (ext)
			{
				case "jpg": case "jpeg":
					f = ImageFormat.Jpeg;
					break;
				case "gif":
					f = ImageFormat.Gif;
					break;
				case "bmp":
					f = ImageFormat.Bmp;
					break;
				case "png":
					f = ImageFormat.Png;
					break;
				case "tif": case "tiff":
					f = ImageFormat.Tiff;
					break;
				default:
					MessageBox.Show("Usupported image format, will use PNG format instead!");
					f = ImageFormat.Png;
					break;
			}
			image.Save (filename, f);
		}

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            //
        }

        Point origin;

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            origin = pictureBox.PointToScreen(e.Location);
            pictureBox.Cursor = Cursors.NoMove2D;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                pictureBox.Cursor = Cursors.Default;
                return;
            }
            Point current = pictureBox.PointToScreen(e.Location);
            Point delta = new Point
            {
                X = current.X - origin.X,
                Y = current.Y - origin.Y,
            };
            if (PerformScroll(panelClient.HorizontalScroll, delta.X))
                origin.X = current.X;
            if (PerformScroll(panelClient.VerticalScroll, delta.Y))
                origin.Y = current.Y;
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox.Cursor = Cursors.Default;
        }

        private bool PerformScroll(ScrollProperties scroll, int delta)
        {
            if (scroll.Maximum - scroll.Minimum <= scroll.LargeChange)
                return false;
            if (Math.Abs(delta) < scroll.SmallChange)
                return false;
            int value = scroll.Value - delta;
            if (value < scroll.Minimum)
                value = scroll.Minimum;
            if (value >= scroll.Maximum - scroll.LargeChange)
                value = scroll.Maximum - scroll.LargeChange;
            scroll.Value = value;
            return true;
        }
    }
}
