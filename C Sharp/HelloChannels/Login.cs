using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace example
{
    public partial class Login : Form
    {
        private HelloChannels helloChannels;

        //Gross, sorry :)
        public Login( HelloChannels channels )
        {
            this.helloChannels = channels;
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            helloChannels.Login(usernameInput.Text, hostInput.Text, Convert.ToInt32( portInput.Text ) );
            this.Hide();
        }
    }
}
