using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotRas;
using System.Net;
using System.Configuration;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading;

namespace VPNClient
{
    public partial class VPNClient : Form
    {
        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RasDeleteEntry(string lpszPhonebook, string lpszEntry);

        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RasGetErrorString(int uErrorValue, string lpszErrorString, int cBufSize);

        private string sEntryName = null;
        private string sEntryId = null;
        private RasConnectionWatcher connWatcher = null;
        private RasConnection conn = null;
        private bool isStopping = false;
        private bool haveStopped = false;
        private ContextMenu notifyiconMnu;

        public VPNClient()
        {
            InitializeComponent();

            // load config setting
            tVpnName.Text = string.IsNullOrWhiteSpace(GetConfigValue("vpnname")) ? "VPN Connection" : GetConfigValue("vpnname");
            tServerIP.Text = GetConfigValue("serverip");
            tUsername.Text = GetConfigValue("username");
            tUserkey.Text = Encoding.UTF8.GetString(Convert.FromBase64String(GetConfigValue("password")));

            // setting UI
            bDisconnect.Enabled = false;
        }

        private void bConnect_Click(object sender, EventArgs e)
        {
            string sServerip = tServerIP.Text;
            string sUsername = tUsername.Text;
            string sPassword = tUserkey.Text;
            sEntryName = tVpnName.Text;

            this.tMessage.Clear();
            CreateVpnEntry(sServerip);

            this.Dialer.EntryName = sEntryName;
            this.Dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);

            try
            {
                this.Dialer.Credentials = new NetworkCredential(sUsername, sPassword);
                var handle = this.Dialer.DialAsync();
                this.tMessage.AppendText("正在尝试连接...\r\n");

                // 连接状态监控
                connWatcher = new RasConnectionWatcher();
                connWatcher.Handle = handle;
                connWatcher.Disconnected += ConnWatcher_Disconnected;
                connWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                this.tMessage.AppendText(ex.ToString());
            }
        }

        /// <summary>
        /// 连接断开处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnWatcher_Disconnected(object sender, RasConnectionEventArgs e)
        {
            if (!haveStopped)
            {
                haveStopped = true;
                DoAfterDisconnected();
            }
        }

        private void CreateVpnEntry(string sVpnIp)
        {
            AllUsersPhoneBook.Open();

            if (this.AllUsersPhoneBook.Entries.Contains(sEntryName))
            {
                //todo
                this.AllUsersPhoneBook.Entries[sEntryName].PhoneNumber = sVpnIp;
            }
            else
            {
                try
                {
                    RasEntry entry = RasEntry.CreateVpnEntry(sEntryName, sVpnIp, RasVpnStrategy.PptpOnly, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn), false);
                    entry.EncryptionType = RasEncryptionType.Optional;

                    this.AllUsersPhoneBook.Entries.Add(entry);
                }
                catch (Exception ex)
                {
                    tMessage.AppendText(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the dialer state changes during a connection attempt.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">An <see cref="DotRas.StateChangedEventArgs"/> containing event data.</param>
        private void Dialer_StateChanged(object sender, StateChangedEventArgs e)
        {
            //this.tMessage.AppendText(e.State.ToString() + "\r\n");
        }

        /// <summary>
        /// Occurs when the dialer has completed a dialing operation.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">An <see cref="DotRas.DialCompletedEventArgs"/> containing event data.</param>
        private void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.tMessage.AppendText("连接取消!\r\n");
            }
            else if (e.TimedOut)
            {
                this.tMessage.AppendText("连接超时!\r\n");
            }
            else if (e.Error != null)
            {
                this.tMessage.AppendText("请检查网络连接或者账户是否异常.\r\n");
            }
            else if (e.Connected)
            {
                this.tMessage.AppendText("连接成功!\r\n");
                bConnect.Enabled = false;
                bDisconnect.Enabled = true;
                bRemoveVpn.Enabled = false;

                // 当前VPN的IP地址
                string vpnIP = GetVpnIP();

                var ifIndex = IPHelper.GetAdapterIndex(sEntryId);

                // 添加路由
                string cmdResult = string.Empty;
                CMDHelper.RunCmd("route add 10.250.1.0 mask 255.255.255.0 10.250.250.1 if " + ifIndex, out cmdResult);
                CMDHelper.RunCmd("route add 10.250.2.0 mask 255.255.255.0 10.250.250.1 if " + ifIndex, out cmdResult);
                CMDHelper.RunCmd("route add 10.250.3.0 mask 255.255.255.0 10.250.250.1 if " + ifIndex, out cmdResult);
                CMDHelper.RunCmd("route add 10.250.4.0 mask 255.255.255.0 10.250.250.1 if " + ifIndex, out cmdResult);
                CMDHelper.RunCmd("route add 10.250.5.0 mask 255.255.255.0 10.250.250.1 if " + ifIndex, out cmdResult);
                CMDHelper.RunCmd("route delete 10.0.0.0", out cmdResult);

                this.tMessage.AppendText("绑定IP:" + vpnIP + "\r\n");

                isStopping = false;
                haveStopped = false;
            }

            if (!e.Connected)
            {
                this.tMessage.AppendText("连接失败！\r\n");
            }
        }

        /// <summary>
        /// 获取当前VPN地址
        /// </summary>
        /// <returns></returns>
        private string GetVpnIP()
        {
            var vpnIP = string.Empty;
            foreach (RasConnection connection in RasConnection.GetActiveConnections())
            {
                if (connection.EntryName == sEntryName)
                {
                    conn = connection;

                    RasIPInfo ipAddresses = (RasIPInfo)connection.GetProjectionInfo(RasProjectionType.IP);
                    if (ipAddresses != null)
                    {
                        vpnIP = ipAddresses.IPAddress.ToString();
                        sEntryId = connection.EntryId.ToString();
                    }
                    break;
                }
            }

            return vpnIP;
        }

        private void bDisconnect_Click(object sender, EventArgs e)
        {
            DoDisconnect();
        }

        private void DoDisconnect()
        {
            if (!isStopping)
            {
                isStopping = true;

                DoMessage("正在断开连接...");

                if (this.Dialer.IsBusy)
                {
                    this.Dialer.DialAsyncCancel();
                }
                else
                {
                    if (conn != null)
                    {

                        conn.HangUp();
                    }
                }
            }
        }

        private void DoAfterDisconnected()
        {
            if (bConnect.InvokeRequired)
            {
                bConnect.Invoke(new Action(() =>
                {
                    bConnect.Enabled = true;
                }));
            }
            else
            {
                bConnect.Enabled = true;
            }

            if (bDisconnect.InvokeRequired)
            {
                bDisconnect.Invoke(new Action(() =>
                {
                    bDisconnect.Enabled = false;
                }));
            }
            else
            {
                bDisconnect.Enabled = false;
            }

            if (bRemoveVpn.InvokeRequired)
            {
                bRemoveVpn.Invoke(new Action(() =>
                {
                    bRemoveVpn.Enabled = true;
                }));
            }
            else
            {
                bRemoveVpn.Enabled = true;
            }

            string cmdResult = string.Empty;
            CMDHelper.RunCmd("route delete 10.250.1.0", out cmdResult);
            CMDHelper.RunCmd("route delete 10.250.2.0", out cmdResult);
            CMDHelper.RunCmd("route delete 10.250.3.0", out cmdResult);
            CMDHelper.RunCmd("route delete 10.250.4.0", out cmdResult);
            CMDHelper.RunCmd("route delete 10.250.5.0", out cmdResult);

            isStopping = false;
            conn = null;
            DoMessage("连接已断开");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SetConfigValue("serverip", tServerIP.Text);
            SetConfigValue("username", tUsername.Text);
            SetConfigValue("password", Convert.ToBase64String(Encoding.UTF8.GetBytes(tUserkey.Text)));
        }

        private static void SetConfigValue(string key, string value)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string GetConfigValue(string key)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] == null)
                return "";
            else
                return config.AppSettings.Settings[key].Value;
        }

        private void Dialer_Error(object sender, System.IO.ErrorEventArgs e)
        {
            this.tMessage.AppendText("连接失败，请检查网络异常！\r\n");
        }

        /// <summary>
        /// 删除条目
        /// </summary>
        /// <param name="strEntryName">条目名称</param>
        /// <param name="strError">错误</param>
        /// <returns>结果</returns>
        private bool DeleteEntry(string strEntryName, out string strError)
        {
            int nErrorValue = RasDeleteEntry(null, strEntryName);
            if (nErrorValue == 0)
            {
                strError = null;
                return true;
            }

            strError = this.GetErrorString(nErrorValue);
            return false;
        }

        /// <summary>
        /// 获得错误
        /// </summary>
        /// <param name="nErrorValue"></param>
        /// <returns></returns>
        private string GetErrorString(int nErrorValue)
        {
            if ((nErrorValue >= 600) && (nErrorValue < 0x2f2))
            {
                int cBufSize = 0x100;
                string lpszErrorString = new string(new char[cBufSize]);
                if (RasGetErrorString(nErrorValue, lpszErrorString, cBufSize) != 0)
                {
                    lpszErrorString = null;
                }
                if ((lpszErrorString != null) && (lpszErrorString.Length > 0))
                {
                    return lpszErrorString;
                }
            }
            return null;
        }

        private void bRemoveVpn_Click(object sender, EventArgs e)
        {
            Thread.Sleep(2000);

            var name = tVpnName.Text.Trim();

            string errMsg = string.Empty;
            var reuslt = DeleteEntry(name, out errMsg);

            if (reuslt)
            {
                DoMessage("移除成功！");
                return;
            }

            DoMessage(errMsg);
        }

        private void DoMessage(string content)
        {
            if (tMessage.InvokeRequired)
            {
                tMessage.Invoke(new Action(() =>
                {
                    tMessage.AppendText(content + "\r\n");
                }));
            }
            else
            {
                tMessage.AppendText(content + "\r\n");
            }
        }

        private void VPNClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (conn != null)
            {
                MessageBox.Show("请先关闭连接，再退出程序！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
        }

        private void VPNClient_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)  //判断是否最小化
            {
                this.ShowInTaskbar = false;  //不显示在系统任务栏
                notifyIcon1.Visible = true;  //托盘图标可见

                notifyIcon1.Visible = true;
                this.Hide();
                this.ShowInTaskbar = false;

                Initializenotifyicon();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
        }

        /// <summary>
        /// 最小化到任务栏
        /// </summary>
        private void Initializenotifyicon()
        {
            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象 
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "显示窗口";
            mnuItms[0].Click += new System.EventHandler(this.notifyIcon1_showfrom);

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出系统";
            mnuItms[2].Click += new System.EventHandler(this.ExitSelect);
            mnuItms[2].DefaultItem = true;

            notifyiconMnu = new ContextMenu(mnuItms);
            notifyIcon1.ContextMenu = notifyiconMnu;
        }

        public void notifyIcon1_showfrom(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
        }

        public void ExitSelect(object sender, System.EventArgs e)
        {
            if (conn != null)
            {
                MessageBox.Show("请先断开连接，再退出程序！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //隐藏托盘程序中的图标 
            notifyIcon1.Visible = false;
            //关闭系统 
            this.Close();
        }
    }
}
