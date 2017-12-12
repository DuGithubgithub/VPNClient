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
using System.IO;
using Microsoft.Win32;

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
        private List<string> addRouteArray = new List<string>();
        private List<string> deleteRouteArray = new List<string>();
        private System.Windows.Forms.Timer connStatusTimer;

        public VPNClient()
        {
            InitializeComponent();

            InitCustomRoute();

            // load config setting
            tVpnName.Text = string.IsNullOrWhiteSpace(ConfigHelper.GetConfigValue("vpnname")) ? "VPN Connection" : ConfigHelper.GetConfigValue("vpnname");
            tServerIP.Text = ConfigHelper.GetConfigValue("serverip");
            tUsername.Text = ConfigHelper.GetConfigValue("username");
            tUserkey.Text = Encoding.UTF8.GetString(Convert.FromBase64String(ConfigHelper.GetConfigValue("password")));
            chbAutoStart.Checked = Convert.ToBoolean(ConfigHelper.GetConfigValue("autostart") == string.Empty ? "false" : ConfigHelper.GetConfigValue("autostart"));
            this.Text = this.Text + " - " + tVpnName.Text;

            InitNotifyicon();

            // setting UI
            bDisconnect.Enabled = false;

            if (chbAutoStart.Checked)
            {
                BeginConnect();
                DoMinimized();
            }

            connStatusTimer = new System.Windows.Forms.Timer();
            connStatusTimer.Interval = 10000;
            connStatusTimer.Tick += ConnStatusTimer_Tick;
            connStatusTimer.Start();
        }

        private void ConnStatusTimer_Tick(object sender, EventArgs e)
        {
            if (conn == null)
            {
                BeginConnect();
            }
        }

        private void InitCustomRoute()
        {
            var routeLines = File.ReadAllLines(Path.Combine(Application.StartupPath, "customroute.txt"));
            if (routeLines.Length > 0)
            {
                foreach (var routeStr in routeLines)
                {
                    if (routeStr.StartsWith("add"))
                    {
                        addRouteArray.Add(routeStr);
                    }

                    if (routeStr.StartsWith("delete"))
                    {
                        deleteRouteArray.Add(routeStr);
                    }
                }
            }
        }

        private void bConnect_Click(object sender, EventArgs e)
        {
            if (chbAutoStart.Checked)
            {
                AddAutoStart();
            }

            BeginConnect();
        }

        private void BeginConnect()
        {
            string sServerip = tServerIP.Text;
            string sUsername = tUsername.Text;
            string sPassword = tUserkey.Text;
            sEntryName = tVpnName.Text;

            this.tMessage.Clear();
            if (!CreateVpnEntry(sServerip))
            {
                return;
            }

            this.Dialer.EntryName = sEntryName;
            this.Dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);

            try
            {
                this.Dialer.Credentials = new NetworkCredential(sUsername, sPassword);
                var handle = this.Dialer.DialAsync();
                DoMessage("正在尝试连接...");

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
            if (e.Connection.EntryName == sEntryName && !haveStopped)
            {
                haveStopped = true;
                DoAfterDisconnected();
            }
        }

        private bool CreateVpnEntry(string sVpnIp)
        {
            AllUsersPhoneBook.Open();

            if (this.AllUsersPhoneBook.Entries.Contains(sEntryName))
            {
                var ipInfo = GetVpnIP();
                if (ipInfo != null)
                {
                    DoMessage("已经存在到同一个服务器的连接！");
                    return false;
                }

                this.AllUsersPhoneBook.Entries[sEntryName].PhoneNumber = sVpnIp;
            }
            else
            {
                try
                {
                    var deviceList = RasDevice.GetDevices();
                    var vpnDevice = deviceList.Where(d => d.DeviceType == RasDeviceType.Vpn && d.Name.Contains("(PPTP)")).FirstOrDefault();
                    if (vpnDevice == null)
                    {
                        DoMessage("无可用VPN端口，请尝试执行[netsh winsock reset]进行修复！");
                        return false;
                    }

                    RasEntry entry = RasEntry.CreateVpnEntry(sEntryName, sVpnIp, RasVpnStrategy.PptpOnly, vpnDevice, false);
                    entry.EncryptionType = RasEncryptionType.Optional;

                    this.AllUsersPhoneBook.Entries.Add(entry);
                }
                catch (Exception ex)
                {
                    DoMessage(ex.ToString());
                }
            }

            return true;
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
                DoMessage("连接取消！");
            }
            else if (e.TimedOut)
            {
                DoMessage("连接超时！");
            }
            else if (e.Error != null)
            {
                DoMessage("请检查网络连接或者账户是否异常！");
            }
            else if (e.Connected)
            {
                DoMessage("连接成功！");
                DoAfterConnected();
            }

            if (!e.Connected)
            {
                DoMessage("连接失败！");
            }
        }

        private void DoAfterConnected()
        {
            if (bConnect.InvokeRequired)
            {
                bConnect.Invoke(new Action(() =>
                {
                    bConnect.Enabled = false;
                }));
            }
            else
            {
                bConnect.Enabled = false;
            }

            if (bDisconnect.InvokeRequired)
            {
                bDisconnect.Invoke(new Action(() =>
                {
                    bDisconnect.Enabled = true;
                }));
            }
            else
            {
                bDisconnect.Enabled = true;
            }

            if (bRemoveVpn.InvokeRequired)
            {
                bRemoveVpn.Invoke(new Action(() =>
                {
                    bRemoveVpn.Enabled = false;
                }));
            }
            else
            {
                bRemoveVpn.Enabled = false;
            }

            var ipInfo = GetVpnIP();

            // 当前VPN的IP地址
            string vpnClientIP = string.Empty;
            string vpnServerIP = string.Empty;
            if (ipInfo != null)
            {
                vpnClientIP = ipInfo.IPAddress.ToString();
                vpnServerIP = ipInfo.ServerIPAddress.ToString();
            }

            var ifIndex = IPHelper.GetAdapterIndex(sEntryId);

            // 添加路由
            string cmdResult = string.Empty;

            if (addRouteArray.Count > 0)
            {
                foreach (var addRoute in addRouteArray)
                {
                    CMDHelper.RunCmd("route " + addRoute + " " + vpnServerIP + " if " + ifIndex, out cmdResult);
                }
            }

            if (deleteRouteArray.Count > 0)
            {
                foreach (var deleteRoute in deleteRouteArray)
                {
                    CMDHelper.RunCmd("route " + deleteRoute, out cmdResult);
                }
            }

            DoMessage("绑定IP:" + vpnClientIP);

            isStopping = false;
            haveStopped = false;
        }

        /// <summary>
        /// 获取当前VPN地址
        /// </summary>
        /// <returns></returns>
        private RasIPInfo GetVpnIP()
        {

            foreach (RasConnection connection in RasConnection.GetActiveConnections())
            {
                if (connection.EntryName == sEntryName)
                {
                    conn = connection;
                    sEntryId = connection.EntryId.ToString();
                    return (RasIPInfo)connection.GetProjectionInfo(RasProjectionType.IP);
                }
            }

            return null;
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

            if (addRouteArray.Count > 0)
            {
                foreach (var addRoute in addRouteArray)
                {
                    var addRouteInfo = addRoute.Split(' ');
                    if (addRouteInfo.Length > 1)
                    {
                        CMDHelper.RunCmd("route delete" + addRouteInfo[1], out cmdResult);
                    }
                }
            }

            isStopping = false;
            conn = null;
            DoMessage("连接已断开");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            ConfigHelper.SetConfigValue("vpnname", tVpnName.Text);
            ConfigHelper.SetConfigValue("serverip", tServerIP.Text);
            ConfigHelper.SetConfigValue("username", tUsername.Text);
            ConfigHelper.SetConfigValue("password", Convert.ToBase64String(Encoding.UTF8.GetBytes(tUserkey.Text)));
            ConfigHelper.SetConfigValue("autostart", chbAutoStart.Checked.ToString().ToLower());

            if (!chbAutoStart.Checked)
            {
                RemoveAutoStart();
            }

            DoDisconnect();
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

        private void VPNClient_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)  //判断是否最小化
            {
                DoMinimized();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
            this.ShowInTaskbar = true;
        }

        private void DoMinimized()
        {
            this.ShowInTaskbar = false;  //不显示在系统任务栏
            this.notifyIcon1.Visible = true;  //托盘图标可见
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// 最小化到任务栏
        /// </summary>
        private void InitNotifyicon()
        {
            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象 
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "显示窗口";
            mnuItms[0].Click += new System.EventHandler(this.notifyIcon1_showfrom);
            mnuItms[0].DefaultItem = true;

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出系统";
            mnuItms[2].Click += new System.EventHandler(this.ExitSelect);

            notifyiconMnu = new ContextMenu(mnuItms);
            notifyIcon1.ContextMenu = notifyiconMnu;
        }

        public void notifyIcon1_showfrom(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
            this.ShowInTaskbar = true;
        }

        public void ExitSelect(object sender, System.EventArgs e)
        {
            DoDisconnect();

            //关闭系统 
            this.Close();
        }

        private void AddAutoStart()
        {
            SetAutoStart(true);
        }

        private void RemoveAutoStart()
        {
            SetAutoStart(false);
        }

        private void SetAutoStart(bool autoStart)
        {
            try
            {
                string appRegName = tVpnName.Text.Replace(" ", "_"); ;
                RegistryKey reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (reg == null)
                {
                    reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                }

                //var exeEntry = Path.Combine(Application.StartupPath, "start.vbs");
                var exeEntry = Application.ExecutablePath;

                //var result = reg.GetValueNames();
                //reg.DeleteValue("VPN Connection", false);

                if (autoStart)
                {
                    if (reg.GetValue(appRegName) == null || reg.GetValue(appRegName).ToString() != exeEntry)
                    {
                        reg.SetValue(appRegName, exeEntry);
                    }
                }
                else
                {
                    reg.DeleteValue(appRegName, false);
                }

                reg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置开机启动失败：" + ex.Message);
            }
        }
    }
}
