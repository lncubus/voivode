namespace voivode
{
    partial class PasswordDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelUser = new System.Windows.Forms.Label();
            this.textBoxUser = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.checkBoxRemember = new System.Windows.Forms.CheckBox();
            this.labelSite = new System.Windows.Forms.Label();
            this.comboBoxSites = new System.Windows.Forms.ComboBox();
            this.labelAlert = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(13, 77);
            this.labelUser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(121, 20);
            this.labelUser.TabIndex = 2;
            this.labelUser.Text = "Пользователь";
            // 
            // textBoxUser
            // 
            this.textBoxUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUser.Location = new System.Drawing.Point(12, 100);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new System.Drawing.Size(414, 26);
            this.textBoxUser.TabIndex = 3;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(13, 144);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(67, 20);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "Пароль";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(12, 167);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(414, 26);
            this.textBoxPassword.TabIndex = 5;
            // 
            // buttonLogin
            // 
            this.buttonLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogin.BackColor = System.Drawing.SystemColors.Control;
            this.buttonLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLogin.Location = new System.Drawing.Point(351, 218);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(75, 39);
            this.buttonLogin.TabIndex = 7;
            this.buttonLogin.Text = "Вход";
            this.buttonLogin.UseVisualStyleBackColor = false;
            this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(420, 77);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(0, 0);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.TabStop = false;
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemember
            // 
            this.checkBoxRemember.AutoSize = true;
            this.checkBoxRemember.Location = new System.Drawing.Point(17, 218);
            this.checkBoxRemember.Name = "checkBoxRemember";
            this.checkBoxRemember.Size = new System.Drawing.Size(155, 24);
            this.checkBoxRemember.TabIndex = 6;
            this.checkBoxRemember.Text = "Запомнить меня";
            this.checkBoxRemember.UseVisualStyleBackColor = true;
            // 
            // labelSite
            // 
            this.labelSite.AutoSize = true;
            this.labelSite.Location = new System.Drawing.Point(13, 9);
            this.labelSite.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSite.Name = "labelSite";
            this.labelSite.Size = new System.Drawing.Size(45, 20);
            this.labelSite.TabIndex = 0;
            this.labelSite.Text = "Игра";
            // 
            // comboBoxSites
            // 
            this.comboBoxSites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSites.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxSites.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxSites.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSites.FormattingEnabled = true;
            this.comboBoxSites.Location = new System.Drawing.Point(12, 32);
            this.comboBoxSites.Name = "comboBoxSites";
            this.comboBoxSites.Size = new System.Drawing.Size(414, 28);
            this.comboBoxSites.TabIndex = 1;
            // 
            // labelAlert
            // 
            this.labelAlert.BackColor = System.Drawing.Color.SeaShell;
            this.labelAlert.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelAlert.ForeColor = System.Drawing.Color.Firebrick;
            this.labelAlert.Location = new System.Drawing.Point(0, 249);
            this.labelAlert.Name = "labelAlert";
            this.labelAlert.Size = new System.Drawing.Size(438, 20);
            this.labelAlert.TabIndex = 9;
            this.labelAlert.Visible = false;
            // 
            // PasswordDialog
            // 
            this.AcceptButton = this.buttonLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(438, 269);
            this.Controls.Add(this.comboBoxSites);
            this.Controls.Add(this.labelSite);
            this.Controls.Add(this.checkBoxRemember);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonLogin);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.labelUser);
            this.Controls.Add(this.labelAlert);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "PasswordDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.TextBox textBoxUser;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxRemember;
        private System.Windows.Forms.Label labelSite;
        private System.Windows.Forms.ComboBox comboBoxSites;
        private System.Windows.Forms.Label labelAlert;
    }
}