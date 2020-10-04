﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Collections.Specialized;
using Jot;
using Jot.DefaultInitializer;
using System.Media;

/*
 * v0.8
 * 
 * 
 * - More chat sound alerts. Also a lower volume option for each, volume slider soon.
 * - New setting to let you add the Twitch Popout chat
 * - Fixed issue with settings not being saved if app was forcibly closed
 * - New setting to allow interaction with the main window source
 * - Can change opacity by right-clicking on the top border
 * - New setting in General to hide the taskbar icon
 * - When loading a webcaptioner link as a new widget, it will load default custom css
 * 
 * v0.7
 * - Added a setting to show/hide the system tray icon control
 * - Added a setting to auto-hide borders when the application launches
 * - Added a setting to enable/disable the confirmation box when closing the application
 * - Added a setting to allow for a notification sound for a new chat message
 * - Can now resize the window on any part of the border
 * - Added a maximize button
 * - Widget windows are a different color from the main window
 * - Updated Chromium Embedded Framework (CEF)
 * - (will require you to install Microsoft Visual C++ 2015-2019 Redistributable x86)
 * - (available at https://aka.ms/vs/16/release/vc_redist.x86.exe)
 * 
 * v0.6
 * - Removed the system tray icon control
 * - Settings reworked under the hood (you'll lose settings from previous versions, sorry!)
 * - You can right-click the icon in the taskbar now to toggle borders/show settings
 * - Context menu added to the border (right-click on the black border at top)
 * - Settings window added (click cogwheel button on border, right click border and choose "Settings", or
 *      right-click on the application in the taskbar and choose "Show Settings" from there)
 * - Can change themes for chat now
 * - Also custom css (for theme "None" or use Custom URL)
 * - Zoom increase/decrease buttons won't close menu now
 * - Added "New Window" under context menu. You can add alerts widgets now (like from streamlabs)
 * 
 * TODO:
 * > Custom CSS for widgets
 * Add opacity and zoom levels into the General settings
 * Save and load different css files
 * Fade out the chat when there's not been a message for awhile (useful if opacity is set high)
 * Cascade the windows when creating a new one
 * Cascade the windows when resetting their positions
 * Easier/better size grip, kinda hard to see it, or click on it right now
 * Hotkey and/or menu item to hide the chat and make it visible again
 * Allowing you to chat from the app, quickly (hotkey to enable it?)
 * Custom javascript
 *
 * 
 * Done:
 x Notification sound for new chat messages
 * 
 */

namespace TransparentTwitchChatWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, BrowserWindow
    {
        SolidColorBrush bgColor;
        int cOpacity = 0;
        bool hiddenBorders = false;
        GeneralSettings genSettings;
        TrackingConfiguration genSettingsTrackingConfig;
        JsCallbackFunctions jsCallbackFunctions;

        //StringCollection custom_windows = new StringCollection();
        List<BrowserWindow> windows = new List<BrowserWindow>();

        public bool BotActivity
        {
            get
            {
                return this.genSettings.ShowBotActivity;
            }
            set
            {
                this.genSettings.ShowBotActivity = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            this.genSettings = new GeneralSettings
            {
                CustomWindows = new StringCollection(),
                Username = string.Empty,
                FadeChat = false,
                FadeTime = "120",
                ShowBotActivity = false,
                ChatNotificationSound = "None",
                ThemeIndex = 1,
                ChatType = 0,
                CustomURL = string.Empty,
                ZoomLevel = 0,
                OpacityLevel = 0,
                AutoHideBorders = false,
                EnableTrayIcon = true,
                ConfirmClose = true,
                HideTaskbarIcon = true,
                AllowInteraction = true
            };

            Services.Tracker.Configure(this).IdentifyAs("State").Apply();
            this.genSettingsTrackingConfig = Services.Tracker.Configure(this.genSettings);
            this.genSettingsTrackingConfig.IdentifyAs("MainWindow").Apply();

            var browserSettings = new BrowserSettings
            {
                FileAccessFromFileUrls = CefState.Enabled,
                UniversalAccessFromFileUrls = CefState.Enabled
            };
            Browser1.BrowserSettings = browserSettings;
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;

            //this.Browser1.RegisterAsyncJsObject("jsCallback", new JsCallbackFunctions());
            this.jsCallbackFunctions = new JsCallbackFunctions();
            Browser1.JavascriptObjectRepository.Register("jsCallback", this.jsCallbackFunctions, isAsync: true, options: BindingOptions.DefaultBinder);
        }

        public bool ProcessCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count == 0)
                return true;
            if ((args.Count > 1))
            {
                //the first index always contains the location of the exe so we need to check the second index
                if ((args[1].ToLowerInvariant() == "/toggleborders"))
                {
                    ToggleBorderVisibility();
                }
                else if ((args[1].ToLowerInvariant() == "/settings"))
                {
                    ShowSettingsWindow();
                }
                else if ((args[1].ToLowerInvariant() == "/resetwindow"))
                {
                    ResetWindowPosition();

                    if (MessageBox.Show("Show settings folder?", "Settings Folder", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                        == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start((Services.Tracker.StoreFactory as Jot.Storage.JsonFileStoreFactory).StoreFolderPath);
                    }
                }
            }

            return true;
        }

        public void drawBorders()
        {
            this.ShowInTaskbar = true;

            var hwnd = new WindowInteropHelper(this).Handle;
            WindowHelper.SetWindowExDefault(hwnd);

            btnClose.Visibility = Visibility.Visible;
            btnMin.Visibility = Visibility.Visible;
            btnMax.Visibility = Visibility.Visible;
            btnHide.Visibility = Visibility.Visible;
            btnSettings.Visibility = Visibility.Visible;

            headerBorder.Background = Brushes.Black;
            this.BorderBrush = Brushes.Black;
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

            hiddenBorders = false;

            this.Topmost = false;
            this.Activate();
            this.Topmost = true;

            this.Browser1.IsEnabled = this.genSettings.AllowInteraction;
        }

        public void hideBorders()
        {
            if (this.genSettings.HideTaskbarIcon)
                this.ShowInTaskbar = false;

            var hwnd = new WindowInteropHelper(this).Handle;
            WindowHelper.SetWindowExTransparent(hwnd);

            btnClose.Visibility = Visibility.Hidden;
            btnMin.Visibility = Visibility.Hidden;
            btnMax.Visibility = Visibility.Hidden;
            btnHide.Visibility = Visibility.Hidden;
            btnSettings.Visibility = Visibility.Hidden;

            headerBorder.Background = Brushes.Transparent;
            this.BorderBrush = Brushes.Transparent;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            hiddenBorders = true;

            this.Topmost = false;
            this.Activate();
            this.Topmost = true;

            this.Browser1.IsEnabled = false;
        }

        public void ToggleBorderVisibility()
        {
            if (hiddenBorders)
                DrawBordersForAllWindows();
            else
                HideBordersForAllWindows();
        }

        public void DrawBordersForAllWindows()
        {
            drawBorders();
            foreach (BrowserWindow win in this.windows)
                win.drawBorders();
        }

        public void HideBordersForAllWindows()
        {
            hideBorders();
            foreach (BrowserWindow win in this.windows)
                win.hideBorders();
        }

        public void ResetWindowPosition()
        {
            this.ResetWindowState();
            foreach (BrowserWindow win in this.windows)
                win.ResetWindowState();
        }

        public void ResetWindowState()
        {
            drawBorders();
            this.WindowState = WindowState.Normal;
            this.Left = 10;
            this.Top = 10;
            this.Height = 500;
            this.Width = 320;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F9)
            {
                ToggleBorderVisibility();
            }
            else if (e.Key == Key.F8)
            {
                ShowSettingsWindow();
            }
        }

        private void headerBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ClickCount == 1)
                this.DragMove();
            else if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
            }
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            ExitApplication();
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void SetCustomChatAddress(string url)
        {
            Browser1.Load(url);
        }

        private void SetChatAddress(string chatChannel)
        {
            string username = chatChannel;
            if (chatChannel.Contains("/"))
                username = chatChannel.Split('/').Last();

            string fade = this.genSettings.FadeTime;
            if (!this.genSettings.FadeChat) { fade = "false"; }

            string theme = string.Empty;
            if ((genSettings.ThemeIndex >= 0) && (genSettings.ThemeIndex < KapChat.Themes.Count))
                theme = KapChat.Themes[genSettings.ThemeIndex];

            string url = @"https://www.nightdev.com/hosted/obschat/?";
            url += @"theme=" + theme;
            url += @"&channel=" + username;
            url += @"&fade=" + fade;
            url += @"&bot_activity=" + this.genSettings.ShowBotActivity.ToString();
            url += @"&prevent_clipping=false";
            Browser1.Load(url);
        }

        public void ShowInputFadeDialogBox()
        {
            Input_Fade inputDialog = new Input_Fade();
            if (inputDialog.ShowDialog() == true)
            {
                string fadeTime = inputDialog.Channel;
                int fadeTimeInt = 0;

                int.TryParse(fadeTime, out fadeTimeInt);

                this.genSettings.FadeChat = (fadeTimeInt > 0);
                this.genSettings.FadeTime = fadeTime;
            }
        }

        public void ExitApplication()
        {
            if (this.genSettings.ConfirmClose)
            {
                if (MessageBox.Show("Sure you want to exit the application?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
            //SystemCommands.CloseWindow(this);
        }

        public void ToggleBotActivitySetting()
        {
            this.genSettings.ShowBotActivity = !this.genSettings.ShowBotActivity;
            SetChatAddress(this.genSettings.Username);
        }

        private void MenuItem_ToggleBorderVisible(object sender, RoutedEventArgs e)
        {
            foreach (BrowserWindow window in this.windows)
            {
                if (window != null)
                {
                    if (this.hiddenBorders)
                        window.drawBorders();
                    else
                        window.hideBorders();
                }
            }
            ToggleBorderVisibility();
        }

        private void MenuItem_ShowSettings(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        private void MenuItem_VisitWebsite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/baffler/Transparent-Twitch-Chat-Overlay/releases");
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            hideBorders();
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //this.contextMenu.IsOpen = false;
            //this.contextMenu.Placement = PlacementMode.MousePoint;
            //this.contextMenu.HorizontalOffset = 0;
            //this.contextMenu.VerticalContentAlignment = 0;
            //this.contextMenu.IsOpen = true;
        }

        private void MenuItem_ZoomIn(object sender, RoutedEventArgs e)
        {
            if (this.Browser1.ZoomInCommand.CanExecute(null))
            {
                if (this.genSettings.ZoomLevel < 4.0)
                {
                    this.Browser1.ZoomInCommand.Execute(null);
                    this.genSettings.ZoomLevel = this.Browser1.ZoomLevel;
                }
            }
        }

        private void MenuItem_ZoomOut(object sender, RoutedEventArgs e)
        {
            if (this.Browser1.ZoomOutCommand.CanExecute(null))
            {
                if (this.genSettings.ZoomLevel > -4.0)
                {
                    this.Browser1.ZoomOutCommand.Execute(null);
                    this.genSettings.ZoomLevel = this.Browser1.ZoomLevel;
                }
            }
        }

        private void MenuItem_ZoomReset(object sender, RoutedEventArgs e)
        {
            if (this.Browser1.ZoomResetCommand.CanExecute(null))
            {
                this.Browser1.ZoomResetCommand.Execute(null);
                this.genSettings.ZoomLevel = this.Browser1.ZoomLevel;
            }
        }

        private void Browser1_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                this.Browser1.Dispatcher.Invoke(new Action(() => { this.Browser1.ZoomLevel = this.genSettings.ZoomLevel; }));

                if (string.IsNullOrEmpty(this.genSettings.CustomCSS))
                {
                    // Fix for KapChat so a long chat message doesn't wrap to a new line
                    if (this.genSettings.ChatType == (int)ChatTypes.KapChat)
                        InsertCustomCSS(@".message { display: inline !important; }");
                }
                else
                    InsertCustomCSS(this.genSettings.CustomCSS);


                if ((this.genSettings.ChatType == (int)ChatTypes.KapChat) && (this.genSettings.ChatNotificationSound.ToLower() != "none"))
                {
                    // Insert JS to play a sound on each chat message
                    string script = @"var oldChatInsert = Chat.insert;
Chat.insert = function() {
    (async function() {
	    await CefSharp.BindObjectAsync('jsCallback');
        jsCallback.playSound();
    })();
    return oldChatInsert.apply(oldChatInsert, arguments);
}";
                    InsertCustomJavaScript(script);
                }
            }
        }

        private void InsertCustomCSS(string CSS)
        {
            string base64CSS = Utilities.Base64Encode(CSS.Replace("\r\n", "").Replace("\t", ""));

            string href = "data:text/css;charset=utf-8;base64," + base64CSS;

            string script = "var link = document.createElement('link');";
            script += "link.setAttribute('rel', 'stylesheet');";
            script += "link.setAttribute('type', 'text/css');";
            script += "link.setAttribute('href', '" + href + "');";
            script += "document.getElementsByTagName('head')[0].appendChild(link);";

            this.Browser1.ExecuteScriptAsync(script);
        }

        private void InsertCustomJavaScript(string JS)
        {
            try
            {
                this.Browser1.ExecuteScriptAsync(JS);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateNewWindow(string URL)
        {
            if (this.genSettings.CustomWindows.Contains(URL))
            {
                MessageBox.Show("That URL already exists as a window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.genSettings.CustomWindows.Add(URL);
                OpenNewCustomWindow(URL);
            }
        }

        private void CreateNewWindowDialog()
        {
            Input_Custom inputDialog = new Input_Custom();
            if (inputDialog.ShowDialog() == true)
            {
                CreateNewWindow(inputDialog.Url);
            }
        }

        private void MenuItem_ClickNewWindow(object sender, RoutedEventArgs e)
        {
            CreateNewWindowDialog();
        }

        private void ShowSettingsWindow()
        {
            WindowSettings config = new WindowSettings {
                Title = "Main Window",
                ChatType = this.genSettings.ChatType,
                URL = this.genSettings.CustomURL,
                Username = this.genSettings.Username,
                ChatFade = this.genSettings.FadeChat,
                FadeTime = this.genSettings.FadeTime,
                ShowBotActivity = this.genSettings.ShowBotActivity,
                ChatNotificationSound = this.genSettings.ChatNotificationSound,
                Theme = this.genSettings.ThemeIndex,
                CustomCSS = this.genSettings.CustomCSS,
                AutoHideBorders = this.genSettings.AutoHideBorders,
                ConfirmClose = this.genSettings.ConfirmClose,
                EnableTrayIcon = this.genSettings.EnableTrayIcon,
                HideTaskbarIcon = this.genSettings.HideTaskbarIcon,
                AllowInteraction = this.genSettings.AllowInteraction
            };

            SettingsWindow settingsWindow = new SettingsWindow(this, config);

            if (settingsWindow.ShowDialog() == true)
            {
                this.genSettings.ChatType = config.ChatType;

                if (config.ChatType == (int)ChatTypes.CustomURL)
                {
                    this.genSettings.CustomURL = config.URL;
                    this.genSettings.CustomCSS = config.CustomCSS;

                    if (!string.IsNullOrEmpty(this.genSettings.CustomURL))
                        SetCustomChatAddress(this.genSettings.CustomURL);
                }
                else if (config.ChatType == (int)ChatTypes.TwitchPopout)
                {
                    this.genSettings.Username = config.Username;
                    this.genSettings.CustomCSS = config.TwitchPopoutCSS;

                    if (!string.IsNullOrEmpty(this.genSettings.Username))
                        SetCustomChatAddress("https://www.twitch.tv/popout/" + this.genSettings.Username + "/chat?popout=");
                }
                else if (config.ChatType == (int)ChatTypes.KapChat)
                {
                    this.genSettings.Username = config.Username;
                    this.genSettings.FadeChat = config.ChatFade;
                    this.genSettings.FadeTime = config.FadeTime;
                    this.genSettings.ShowBotActivity = config.ShowBotActivity;
                    this.genSettings.ChatNotificationSound = config.ChatNotificationSound;
                    this.genSettings.ThemeIndex = config.Theme;

                    if (this.genSettings.ThemeIndex == 0)
                        this.genSettings.CustomCSS = config.CustomCSS;
                    else
                        this.genSettings.CustomCSS = string.Empty;


                    if (!string.IsNullOrEmpty(this.genSettings.Username))
                        SetChatAddress(this.genSettings.Username);


                    if (this.genSettings.ChatNotificationSound.ToLower() == "none")
                        this.jsCallbackFunctions.MediaFile = string.Empty;
                    else
                    {
                        Uri startupPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                        string file = System.IO.Path.GetDirectoryName(startupPath.LocalPath) + "\\assets\\" + this.genSettings.ChatNotificationSound + ".wav";
                        if (System.IO.File.Exists(file))
                        {
                            this.jsCallbackFunctions.MediaFile = file;
                        }
                    }
                }

                this.genSettings.AutoHideBorders = config.AutoHideBorders;
                this.genSettings.ConfirmClose = config.ConfirmClose;
                this.genSettings.EnableTrayIcon = config.EnableTrayIcon;
                this.genSettings.HideTaskbarIcon = config.HideTaskbarIcon;
                this.genSettings.AllowInteraction = config.AllowInteraction;

                this.taskbarControl.Visibility = config.EnableTrayIcon ? Visibility.Visible : Visibility.Hidden;
                this.ShowInTaskbar = !config.HideTaskbarIcon;

                if (!this.hiddenBorders) this.Browser1.IsEnabled = config.AllowInteraction;

                // Save the new changes for settings
                this.genSettingsTrackingConfig.Persist();
            }
        }

        private void OpenNewCustomWindow(string url, bool hideBorder = false)
        {
            CustomWindow newWindow = new CustomWindow(this, url);
            windows.Add(newWindow);
            newWindow.Show();
            
            if (hideBorder)
                newWindow.hideBorders();
        }

        public void RemoveCustomWindow(string url)
        {
            if (this.genSettings.CustomWindows.Contains(url))
            {
                this.genSettings.CustomWindows.Remove(url);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.genSettings.EnableTrayIcon)
                this.taskbarControl.Visibility = Visibility.Hidden;

            SetupBrowser();
        }

        private void Browser1_Initialized(object sender, EventArgs e)
        {
            
        }

        private void SetupBrowser()
        {
            if (!this.Browser1.IsInitialized)
            {
                MessageBox.Show(
                  "Error setting up source. The component was not initialized",
                  "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK
                );
                return;
            }

            this.bgColor = new SolidColorBrush(Color.FromArgb(this.genSettings.OpacityLevel, 0, 0, 0));
            this.Background = this.bgColor;
            this.cOpacity = this.genSettings.OpacityLevel;

            if (this.genSettings.AutoHideBorders)
                hideBorders();
            else
                drawBorders();

            if (this.genSettings.ChatNotificationSound.ToLower() != "none")
            {
                Uri startupPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                string file = System.IO.Path.GetDirectoryName(startupPath.LocalPath) + "\\assets\\" + this.genSettings.ChatNotificationSound + ".wav";
                if (System.IO.File.Exists(file))
                {
                    this.jsCallbackFunctions.MediaFile = file;
                }
            }

            if (this.genSettings.CustomWindows != null)
            {
                foreach (string url in this.genSettings.CustomWindows)
                    OpenNewCustomWindow(url, this.genSettings.AutoHideBorders);
            }

            this.Browser1.ZoomLevelIncrement = 0.25;

            if ((this.genSettings.ChatType == (int)ChatTypes.CustomURL) && (!string.IsNullOrWhiteSpace(this.genSettings.CustomURL)))
            {
                SetCustomChatAddress(this.genSettings.CustomURL);
            }
            else if ((this.genSettings.ChatType == (int)ChatTypes.TwitchPopout) && (!string.IsNullOrEmpty(this.genSettings.Username)))
            {
                SetCustomChatAddress("https://www.twitch.tv/popout/" + this.genSettings.Username + "/chat?popout=");
            }
            else if (!string.IsNullOrWhiteSpace(this.genSettings.Username))
            {
                SetChatAddress(this.genSettings.Username);
            }
            else
            {
                Uri startupPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                string address = System.IO.Path.GetDirectoryName(startupPath.LocalPath) + "\\index.html";
                Browser1.Load(address);

                //CefSharp.WebBrowserExtensions.LoadHtml(Browser1,
                //"<html><body style=\"font-size: x-large; color: white; text-shadow: -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000, 1px 1px 0 #000; \">Load a channel to connect to by right-clicking the tray icon.<br /><br />You can move and resize the window, then press the [o] button to hide borders, or use the tray icon menu.</body></html>");
            }
        }

        private void MenuItem_SettingsClick(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void MenuItem_ResetWindowClick(object sender, RoutedEventArgs e)
        {
            ResetWindowPosition();

            if (MessageBox.Show("Show settings folder?", "Settings Folder", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start((Services.Tracker.StoreFactory as Jot.Storage.JsonFileStoreFactory).StoreFolderPath);
            }
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void MenuItem_IncOpacity(object sender, RoutedEventArgs e)
        {
            this.cOpacity += 15;
            if (this.cOpacity > 255) this.cOpacity = 255;
            this.genSettings.OpacityLevel = (byte)this.cOpacity;
            this.bgColor.Color = Color.FromArgb((byte)this.cOpacity, 0, 0, 0);
        }

        private void MenuItem_DecOpacity(object sender, RoutedEventArgs e)
        {
            this.cOpacity -= 15;
            if (this.cOpacity < 0) this.cOpacity = 0;
            this.genSettings.OpacityLevel = (byte)this.cOpacity;
            this.bgColor.Color = Color.FromArgb((byte)this.cOpacity, 0, 0, 0);
        }

        private void MenuItem_ResetOpacity(object sender, RoutedEventArgs e)
        {
            this.cOpacity = 0;
            this.genSettings.OpacityLevel = 0;
            this.bgColor.Color = Color.FromArgb(0, 0, 0, 0);
        }
    }

    public class JsCallbackFunctions
    {
        public string MediaFile;

        public JsCallbackFunctions()
        {
            this.MediaFile = "";
        }

        public void playSound()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.MediaFile))
                {
                    SoundPlayer sp = new SoundPlayer(this.MediaFile);
                    //MediaPlayer mp = new MediaPlayer();
                    //mp.Open(new Uri(mediaFile));
                    //mp.Volume = 0.5f;
                    sp.Play();
                }
            } catch { }
        }
    }

    public class GeneralSettings
    {
        [Trackable]
        public StringCollection CustomWindows { get; set; }
        [Trackable]
        public string Username { get; set; }
        [Trackable]
        public bool FadeChat { get; set; }
        [Trackable]
        public string FadeTime { get; set; } // fade time in seconds or "false"
        [Trackable]
        public bool ShowBotActivity { get; set; }
        [Trackable]
        public string ChatNotificationSound { get; set; }
        [Trackable]
        public int ThemeIndex { get; set; }
        [Trackable]
        public string CustomCSS { get; set; }
        [Trackable]
        public int ChatType { get; set; }
        [Trackable]
        public string CustomURL { get; set; }
        [Trackable]
        public double ZoomLevel { get; set; }
        [Trackable]
        public byte OpacityLevel { get; set; }
        [Trackable]
        public bool AutoHideBorders { get; set; }
        [Trackable]
        public bool EnableTrayIcon { get; set; }
        [Trackable]
        public bool ConfirmClose { get; set; }
        [Trackable]
        public bool HideTaskbarIcon { get; set; }
        [Trackable]
        public bool AllowInteraction { get; set; }
    }
}
