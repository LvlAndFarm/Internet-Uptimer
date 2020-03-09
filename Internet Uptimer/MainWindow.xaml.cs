using System;
using System.Windows;
//using System.Windows.Data;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Threading;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Internet_Uptimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        public Thread Poller;
        public bool IsOnline = true;
        public bool IsPolling = true;

        public bool IsClosing = false;

        public MainWindow()
        {
            InitializeComponent();

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            m_notifyIcon.BalloonTipTitle = "Internet Uptimer";
            m_notifyIcon.Text = "Internet Uptimer";
            m_notifyIcon.Icon = new System.Drawing.Icon("hiclipart.com.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            m_notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(
                new MenuItem[] {
                    new MenuItem("Show", m_notifyIcon_Click),
                    new MenuItem("Exit", (e, b) => {
                        Dispatcher.Invoke(() => {
                            IsClosing= true;
                            this.Close();
                        });
                    })
                }
            );

            ShowTrayIcon(true);

            Poller = new Thread(() =>
            {

                while(IsPolling)
                {
                    if (PingTest() != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        if (IsOnline)
                        {
                            NotifyUptime(false);
                        }
                        Dispatcher.Invoke(() =>
                        {
                            IsOnline = false;
                            onlineTxt.Visibility = Visibility.Hidden;
                            offlineTxt.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        if (!IsOnline)
                        {
                            NotifyUptime(true);
                        }
                        Dispatcher.Invoke(() =>
                        {
                            IsOnline = true;
                            onlineTxt.Visibility = Visibility.Visible;
                            offlineTxt.Visibility = Visibility.Hidden;
                        });
                    }

                    Thread.Sleep(3000);
                }
            });
            Poller.Start();
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        private void m_notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        public static IPStatus PingTest()
        {
            //return IPStatus.DestinationHostUnreachable;
            var ping = new Ping();
            PingReply result;
            try
            {
                result = ping.Send("www.google.com");
            } catch
            {
                return IPStatus.DestinationHostUnreachable;
            }
            ping.Dispose();
            return result.Status;

        }

        public static void NotifyUptime(bool isOnline)
        {
            var title = isOnline ? "Connection restored" : "Internet down";
            var text = isOnline ? "Is your app not working? The server is probably down." :
                "Uh-oh, please wait for internet to be restored or restart your router";

            CreateToast(title, text);
        }

        private const String APP_ID = "LvlAndFarm.InternetUptimer";

        public static void CreateToast(string title, string text)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText03);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            for (int i = 0; i < stringElements.Length; i++)
            {
                var xmlText = "";
                switch (i) {
                    case 0:
                        xmlText = title;
                        break;
                    case 1:
                        xmlText = text;
                        break;
                    default:
                        xmlText = "undefined";
                        break;
                }
                stringElements[i].AppendChild(toastXml.CreateTextNode(xmlText));
            }

            //// Specify the absolute path to an image
            //String imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");
            //XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            //imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsClosing)
            {
                e.Cancel = true;
                this.Hide();
                m_notifyIcon.ShowBalloonTip(3000);
            } else
            {
                Poller.Abort();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ShowTrayIcon(false);
            m_notifyIcon.Dispose();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsClosing = true;
            //Poller.Abort();
            this.Close();
        }
    }
}
