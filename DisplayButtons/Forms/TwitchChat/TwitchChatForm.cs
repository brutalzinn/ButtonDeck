﻿
using BackendAPI.Objects;
using DisplayButtons.BackendAPI.Objects;
using DisplayButtons.Bibliotecas.Helpers;
using DisplayButtons.Bibliotecas.OAuthConsumer;
using DisplayButtons.Bibliotecas.OAuthConsumer.Auths;
using DisplayButtons.Bibliotecas.TwitchWrapper;
using Microsoft.Win32;
using OpenQA.Selenium.Remote;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace DisplayButtons.Forms.TwitchChat
{
    public partial class TwitchChatForm : TemplateForm
    {
        private  TwitchClient client;
        private  TwitchAPI api;

        private  TwitchAuth myTwitchApi;
        private static  TwitchChatForm instance;
  
        public static TwitchChatForm Instance
        {
            get
            {
                return instance;
            }
        }
        public TwitchChatForm()
        {
            instance = this;
            InitializeComponent();
            TwitchChatForm.Instance.StartTwitchApi();
            

        }
        private void CloseWithResult(DialogResult result)
        {
            DialogResult = result;
            Close();
        }
        private void TwitchChatForm_Load(object sender, EventArgs e)
        {
            
        }

        private void imageModernButton1_Click(object sender, EventArgs e)
        {
            CloseWithResult(DialogResult.OK);
        }

        public async void StartTwitchApi()
        {

            myTwitchApi = new TwitchAuth();
            await myTwitchApi.AuthCheck();
        // await TaskEx.WaitUntil(myTwitchApi.HasToken);
               api = new TwitchAPI();
            api.Settings.ClientId = myTwitchApi.ClientId();
            api.Settings.AccessToken = myTwitchApi.Token();
             StartChat();
           

        }
     
    
            public  string getUserNameApi()
        {

            return api.Helix.Users.GetUsersAsync().Result.Users.FirstOrDefault().DisplayName;

        }
         public void InitializeAsync(string username)
        {

            EnsureBrowserEmulationEnabled("DisplayButtons.exe");
            WebBrowser teste = new WebBrowser();
            teste.Dock = DockStyle.Fill;
            TwitchChatForm.Instance.Invoke(new Action(() =>
            {
                TwitchChatForm.Instance.panel1.Controls.Add(teste);

            }));

            teste.Navigate($"https://twitch.tv/popout/{username}/chat");





        }
        public void StartChat()
        {

         
          
            if (client != null)
            {
                client = null;
            }

            ConnectionCredentials credentials = new ConnectionCredentials(getUserNameApi(), myTwitchApi.Token());
            var clientOptions = new ClientOptions
            {
            
                MessagesAllowedInPeriod = 750,
                UseSsl = true,
               
                ThrottlingPeriod = TimeSpan.FromSeconds(30)

            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, getUserNameApi(),'!');

            client.OnLog += Client_OnLog;
     //       client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
        client.OnUserJoined += Client_OnUserJoined;
         client.OnUserLeft += Client_OnUserLeft;
        //    client.OnUserStateChanged += ClientUsers;

            client.Connect();
       

         



        }
        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            Debug.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
        private static void Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            TwitchChatForm.Instance.Invoke(new Action(() =>
            {
                TwitchChatForm.Instance.listBox1.Items.Remove(e.Username);

            }));


        }
        private static void ClientUsers(object sender, OnUserStateChangedArgs e)
        {
            TwitchChatForm.Instance.Invoke(new Action(() =>
            {
           

                    TwitchChatForm.Instance.listBox1.Items.Add(e.UserState.DisplayName);
                
                

            }));


        }
        private static void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            TwitchChatForm.Instance.Invoke(new Action(() =>
            {
                TwitchChatForm.Instance.listBox1.Items.Add(e.Username);

            }));


        }
        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
  
         
        
        }

        private static void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
          
    //      Debug.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
  //client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!" + e.BotUsername);
        }
        private void CreateMute(string username)
        {
            dynamic form = Activator.CreateInstance(AbstractDeckAction.FindType("DisplayButtons.Forms.TwitchChat.TwitchUserInput")) as Form;




            if (form.ShowDialog() == DialogResult.OK)
            {
                string motivo = form.motivo_ritchtextbox.Text;
                int timer = Convert.ToInt32(form.timer_textbox.Text);
                client.SendMessage(getUserNameApi(), $"!timeout  {username} {timer} {motivo}");

            }
            else
            {
                form.Close();
            }
        }
        private void CreateBan(string username)
        {
            dynamic form = Activator.CreateInstance(AbstractDeckAction.FindType("DisplayButtons.Forms.TwitchChat.TwitchUserInput")) as Form;




            if (form.ShowDialog() == DialogResult.OK)
            {
                string motivo = form.motivo_ritchtextbox.Text;
                int timer = Convert.ToInt32(form.timer_textbox.Text);
                client.SendMessage(getUserNameApi(), $"!ban {username} {motivo}");
            }
            else
            {
                form.Close();
            }
        }
        private  static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            TwitchChatForm.Instance.Invoke(new Action(() =>
            {
             //   TwitchChatForm.Instance.richTextBox1.AppendText("[" + e.ChatMessage.DisplayName + "]: " + e.ChatMessage.Message + Environment.NewLine);
                // Debug.WriteLine("Received message of" + e.ChatMessage.DisplayName + " Message body: " + e.ChatMessage.Message);
                //    if (e.ChatMessage.Message.Contains("badword"))
                //        client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(30), "Bad word! 30 minute timeout!");
                //
            }));
        }

        private static void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {

            Debug.WriteLine("Received message of" + e.WhisperMessage.Username + " Message body: " + e.WhisperMessage.Message);
            if (e.WhisperMessage.Username == "Saintjoow")
                TwitchChatForm.Instance.client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private static void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                TwitchChatForm.Instance.client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            else
                TwitchChatForm.Instance.client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = this.listBox1.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    contextMenuStrip1.Show(listBox1,e.Location);
                    muteMenuItem1.Click += (sender, e) =>
                    {
                        CreateMute(listBox1.SelectedItem.ToString());
                    };
                    banStripMenuItem1.Click += (sender, e) =>
                    {
                        CreateBan(listBox1.SelectedItem.ToString());
                    };
                  
                    // Do something
                }
            }
        }
        public static void EnsureBrowserEmulationEnabled(string exename = "MarkdownMonster.exe", bool uninstall = false)
        {

            try
            {
                using (
                    var rk = Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)
                )
                {
                    if (!uninstall)
                    {
                        dynamic value = rk.GetValue(exename);
                        if (value == null)
                            rk.SetValue(exename, (uint)11001, RegistryValueKind.DWord);
                    }
                    else
                        rk.DeleteValue(exename);
                }
            }
            catch
            {
            }
        }
        private void imageModernButton2_Click(object sender, EventArgs e)
        {
            client.SendMessage(getUserNameApi(), richTextBox2.Text);
            richTextBox2.Clear();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
