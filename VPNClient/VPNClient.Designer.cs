namespace VPNClient
{
    partial class VPNClient
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VPNClient));
            this.AllUsersPhoneBook = new DotRas.RasPhoneBook(this.components);
            this.Dialer = new DotRas.RasDialer(this.components);
            this.SettingBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chbAutoStart = new System.Windows.Forms.CheckBox();
            this.tVpnName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tUserkey = new System.Windows.Forms.TextBox();
            this.lUserKey = new System.Windows.Forms.Label();
            this.tUsername = new System.Windows.Forms.TextBox();
            this.lUsername = new System.Windows.Forms.Label();
            this.tServerIP = new System.Windows.Forms.TextBox();
            this.lServerIP = new System.Windows.Forms.Label();
            this.bConnect = new System.Windows.Forms.Button();
            this.bDisconnect = new System.Windows.Forms.Button();
            this.tMessage = new System.Windows.Forms.TextBox();
            this.bRemoveVpn = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.SettingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Dialer
            // 
// TODO: “”的代码生成失败，原因是出现异常“无效的基元类型: System.IntPtr。请考虑使用 CodeObjectCreateExpression。”。
            this.Dialer.Credentials = null;
            this.Dialer.EapOptions = new DotRas.RasEapOptions(false, false, false);
            this.Dialer.Options = new DotRas.RasDialOptions(false, false, false, false, false, false, false, false, false, false);
            this.Dialer.StateChanged += new System.EventHandler<DotRas.StateChangedEventArgs>(this.Dialer_StateChanged);
            this.Dialer.DialCompleted += new System.EventHandler<DotRas.DialCompletedEventArgs>(this.Dialer_DialCompleted);
            this.Dialer.Error += new System.EventHandler<System.IO.ErrorEventArgs>(this.Dialer_Error);
            // 
            // SettingBox
            // 
            this.SettingBox.Controls.Add(this.label2);
            this.SettingBox.Controls.Add(this.chbAutoStart);
            this.SettingBox.Controls.Add(this.tVpnName);
            this.SettingBox.Controls.Add(this.label1);
            this.SettingBox.Controls.Add(this.tUserkey);
            this.SettingBox.Controls.Add(this.lUserKey);
            this.SettingBox.Controls.Add(this.tUsername);
            this.SettingBox.Controls.Add(this.lUsername);
            this.SettingBox.Controls.Add(this.tServerIP);
            this.SettingBox.Controls.Add(this.lServerIP);
            this.SettingBox.Location = new System.Drawing.Point(24, 15);
            this.SettingBox.Margin = new System.Windows.Forms.Padding(6);
            this.SettingBox.Name = "SettingBox";
            this.SettingBox.Padding = new System.Windows.Forms.Padding(6);
            this.SettingBox.Size = new System.Drawing.Size(536, 324);
            this.SettingBox.TabIndex = 2;
            this.SettingBox.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 269);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 24);
            this.label2.TabIndex = 9;
            this.label2.Text = "开机启动：";
            // 
            // chbAutoLink
            // 
            this.chbAutoStart.AutoSize = true;
            this.chbAutoStart.Location = new System.Drawing.Point(166, 270);
            this.chbAutoStart.Name = "chbAutoLink";
            this.chbAutoStart.Size = new System.Drawing.Size(28, 27);
            this.chbAutoStart.TabIndex = 8;
            this.chbAutoStart.UseVisualStyleBackColor = true;
            // 
            // tVpnName
            // 
            this.tVpnName.Location = new System.Drawing.Point(166, 46);
            this.tVpnName.Margin = new System.Windows.Forms.Padding(6);
            this.tVpnName.Name = "tVpnName";
            this.tVpnName.Size = new System.Drawing.Size(328, 35);
            this.tVpnName.TabIndex = 7;
            this.tVpnName.Text = "Just A VPN Connection";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 54);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "连接名称：";
            // 
            // tUserkey
            // 
            this.tUserkey.Location = new System.Drawing.Point(166, 209);
            this.tUserkey.Margin = new System.Windows.Forms.Padding(6);
            this.tUserkey.Name = "tUserkey";
            this.tUserkey.PasswordChar = '*';
            this.tUserkey.Size = new System.Drawing.Size(328, 35);
            this.tUserkey.TabIndex = 5;
            // 
            // lUserKey
            // 
            this.lUserKey.AutoSize = true;
            this.lUserKey.Location = new System.Drawing.Point(24, 217);
            this.lUserKey.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lUserKey.Name = "lUserKey";
            this.lUserKey.Size = new System.Drawing.Size(130, 24);
            this.lUserKey.TabIndex = 4;
            this.lUserKey.Text = "用户密码：";
            // 
            // tUsername
            // 
            this.tUsername.Location = new System.Drawing.Point(166, 155);
            this.tUsername.Margin = new System.Windows.Forms.Padding(6);
            this.tUsername.Name = "tUsername";
            this.tUsername.Size = new System.Drawing.Size(328, 35);
            this.tUsername.TabIndex = 3;
            // 
            // lUsername
            // 
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(24, 163);
            this.lUsername.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(130, 24);
            this.lUsername.TabIndex = 2;
            this.lUsername.Text = "用户账号：";
            // 
            // tServerIP
            // 
            this.tServerIP.Location = new System.Drawing.Point(166, 101);
            this.tServerIP.Margin = new System.Windows.Forms.Padding(6);
            this.tServerIP.Name = "tServerIP";
            this.tServerIP.Size = new System.Drawing.Size(328, 35);
            this.tServerIP.TabIndex = 1;
            // 
            // lServerIP
            // 
            this.lServerIP.AutoSize = true;
            this.lServerIP.Location = new System.Drawing.Point(24, 109);
            this.lServerIP.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lServerIP.Name = "lServerIP";
            this.lServerIP.Size = new System.Drawing.Size(130, 24);
            this.lServerIP.TabIndex = 0;
            this.lServerIP.Text = "服务地址：";
            // 
            // bConnect
            // 
            this.bConnect.Location = new System.Drawing.Point(48, 372);
            this.bConnect.Margin = new System.Windows.Forms.Padding(6);
            this.bConnect.Name = "bConnect";
            this.bConnect.Size = new System.Drawing.Size(150, 46);
            this.bConnect.TabIndex = 3;
            this.bConnect.Text = "连接";
            this.bConnect.UseVisualStyleBackColor = true;
            this.bConnect.Click += new System.EventHandler(this.bConnect_Click);
            // 
            // bDisconnect
            // 
            this.bDisconnect.Location = new System.Drawing.Point(230, 372);
            this.bDisconnect.Margin = new System.Windows.Forms.Padding(6);
            this.bDisconnect.Name = "bDisconnect";
            this.bDisconnect.Size = new System.Drawing.Size(150, 46);
            this.bDisconnect.TabIndex = 4;
            this.bDisconnect.Text = "断开";
            this.bDisconnect.UseVisualStyleBackColor = true;
            this.bDisconnect.Click += new System.EventHandler(this.bDisconnect_Click);
            // 
            // tMessage
            // 
            this.tMessage.Location = new System.Drawing.Point(24, 451);
            this.tMessage.Margin = new System.Windows.Forms.Padding(6);
            this.tMessage.Multiline = true;
            this.tMessage.Name = "tMessage";
            this.tMessage.ReadOnly = true;
            this.tMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tMessage.Size = new System.Drawing.Size(536, 170);
            this.tMessage.TabIndex = 6;
            // 
            // bRemoveVpn
            // 
            this.bRemoveVpn.Location = new System.Drawing.Point(410, 372);
            this.bRemoveVpn.Margin = new System.Windows.Forms.Padding(6);
            this.bRemoveVpn.Name = "bRemoveVpn";
            this.bRemoveVpn.Size = new System.Drawing.Size(150, 46);
            this.bRemoveVpn.TabIndex = 7;
            this.bRemoveVpn.Text = "清除设置";
            this.bRemoveVpn.UseVisualStyleBackColor = true;
            this.bRemoveVpn.Click += new System.EventHandler(this.bRemoveVpn_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "VPN连接器";
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // VPNClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 655);
            this.Controls.Add(this.bRemoveVpn);
            this.Controls.Add(this.tMessage);
            this.Controls.Add(this.bDisconnect);
            this.Controls.Add(this.bConnect);
            this.Controls.Add(this.SettingBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "VPNClient";
            this.Text = "VPN连接器";
            this.SizeChanged += new System.EventHandler(this.VPNClient_SizeChanged);
            this.SettingBox.ResumeLayout(false);
            this.SettingBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox SettingBox;
        private System.Windows.Forms.Button bConnect;
        private System.Windows.Forms.Button bDisconnect;
        private System.Windows.Forms.TextBox tMessage;
        private System.Windows.Forms.TextBox tUserkey;
        private System.Windows.Forms.Label lUserKey;
        private System.Windows.Forms.TextBox tUsername;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.TextBox tServerIP;
        private System.Windows.Forms.Label lServerIP;

        private DotRas.RasPhoneBook AllUsersPhoneBook;
        private DotRas.RasDialer Dialer;
        private System.Windows.Forms.TextBox tVpnName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button bRemoveVpn;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chbAutoStart;
    }
}

