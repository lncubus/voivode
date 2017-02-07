using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace voivode
{
    public partial class PasswordDialog : Form
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Host))
                ActiveControl = comboBoxSites;
            if (string.IsNullOrEmpty(Username))
                ActiveControl = textBoxUser;
            else if (string.IsNullOrEmpty(Password))
                ActiveControl = textBoxPassword;
            else
                DialogResult = DialogResult.OK;
        }

        public string[] Hosts
        {
            set
            {
                comboBoxSites.Items.Clear();
                comboBoxSites.Items.AddRange(value);
            }
        }

        public string Host
        {
            get
            {
                return comboBoxSites.Text;
            }
            set
            {
                comboBoxSites.Text = value;
            }
        }

        public string Username
        {
            get { return textBoxUser.Text; }
            set { textBoxUser.Text = value; }
        }

        public string Password
        {
            get { return textBoxPassword.Text; }
            set { textBoxPassword.Text = value; }
        }

        public bool Remember
        {
            get { return checkBoxRemember.Checked; }
            set { checkBoxRemember.Checked = value; }
        }
    }
}
