using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        ITestService service;
        public Form1()
        {
            InitializeComponent();

            var clientConnect = new CRL.DynamicWebApi.ApiClientConnect("http://localhost:8019");

            service = clientConnect.GetClient<ITestService>();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestFactory.RunTest(service);
        }
    }
}
