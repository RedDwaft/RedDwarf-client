using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DarkstarSharp;

namespace example
{
    public partial class HelloChannels : Form, SimpleClientListener
    {
        public SimpleClient client = null;
        private string userName = null;
        private bool loggedIn = false;
        private Dictionary<string, ExampleChannelListener> channels = new Dictionary<string, ExampleChannelListener>();
        public delegate void AppendToConsoleDelegate(string text);
        delegate int AddComboDelegate(ChannelBoxItem item);

        private const string DIRECT_SEND = "<DIRECT>";

        public HelloChannels()
        {
            InitializeComponent();
        }

        public void Login(string user, string host, int port)
        {
            this.userName = user;
            client = new SimpleClient(this);
            client.login(host, port);
        }

        private void HelloChannels_Load(object sender, EventArgs e)
        {
            //Populate "<DIRECT>" channel
            channelCombo.Items.Add(new ChannelBoxItem(DIRECT_SEND));
            channelCombo.SelectedIndex = 0;
            new Login(this).ShowDialog();
        }

        public void AppendToConsole(string text)
        {
            text += Environment.NewLine;
            console.Invoke(new AppendToConsoleDelegate(console.AppendText), new object[] { text });
        }

        void AddComboItem(ChannelBoxItem item)
        {  
            channelCombo.Invoke(new AddComboDelegate(channelCombo.Items.Add), new object[] { item });
        }

        #region SimpleClientListener Members

        public void LoggedIn(byte[] reconnectKey)
        {
            loggedIn = true;
            AppendToConsole("Logged in!");
        }

        public void LoginFailed(string reason)
        {
            AppendToConsole("Login Failed: " + reason);
            loggedIn = false;
        }

        public void ReceivedMessage(byte[] message)
        {
            AppendToConsole(Encoding.UTF8.GetString(message));
        }

        public void Disconnected(bool forced, string message)
        {
            AppendToConsole("Disconnected from server: " + message);
            loggedIn = false;
        }

        public PasswordAuthentication GetPasswordAuthentication()
        {
            return new PasswordAuthentication(userName, "D4rkSt4r5h4rP");
        }

        public ClientChannelListener JoinedChannel(ClientChannel channel)
        {
            ExampleChannelListener c = new ExampleChannelListener(this,channel);
            channels.Add(channel.Name, c);
            AddComboItem(new ChannelBoxItem(c));
            AppendToConsole("Joined channel: " + channel.Name);
            return c;
        }

        #endregion

        private void channelCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChannelBoxItem item = (ChannelBoxItem)channelCombo.SelectedItem;
            AppendToConsole("Selected channel: " + item.Name);
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChannelBoxItem item = (ChannelBoxItem)channelCombo.SelectedItem;
                if( DIRECT_SEND.Equals( item.Name ) )
                {
                    client.SessionSend( Encoding.UTF8.GetBytes(inputBox.Text) );
                }
                else 
                {
                    item.Value.Send(inputBox.Text);
                }
                
                inputBox.Text = "";
            }
        }
    }


    class ExampleChannelListener : ClientChannelListener
    {
        #region ClientChannelListener Members

        public ClientChannel Channel;
        private HelloChannels hello;

        public ExampleChannelListener(HelloChannels hello, ClientChannel channel)
        {
            this.Channel = channel;
            this.hello = hello;
        }

        public void Send(string message)
        {
            Channel.Send(Encoding.UTF8.GetBytes(message));
        }

        public void ReceivedMessage(ClientChannel channel, byte[] message)
        {
            hello.AppendToConsole("[" + channel.Name + "] " + Encoding.UTF8.GetString(message));
        }

        public void LeftChannel(ClientChannel channel)
        {
            hello.AppendToConsole("Left channel " + channel.Name);
        }

        #endregion
    }


    class ChannelBoxItem
    {
        public ChannelBoxItem(string name)
        {
            this.Name = name;
        }

        public ChannelBoxItem(ExampleChannelListener value)
        {
            this.Name = value.Channel.Name;
            this.Value = value;
        }

        public string Name;
        public ExampleChannelListener Value;

        public override string ToString()
        {
            return Name;
        }
    }


}
