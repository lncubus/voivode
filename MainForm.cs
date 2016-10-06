﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Login())
                Close();
            else
                LoadModel();
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
            textBoxTitle.Text = _model.Title;
        }
    }
}
