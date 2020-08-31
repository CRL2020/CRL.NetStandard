using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoteForCode
{
    public partial class Form1 : Form
    {
        private string file = AppDomain.CurrentDomain.BaseDirectory + "__Note.txt";
        public Form1()
        {
            DateTime now = DateTime.Now;
            DateTime dateTime = new DateTime(now.Year, now.Month, 1);
            this.InitializeComponent();
            if (File.Exists(this.file))
            {
                this.txtContent.Text = File.ReadAllText(this.file, Encoding.GetEncoding("gb2312"));
            }
            this.FormClosed += (s, e) =>
            {
                File.WriteAllText(this.file, this.txtContent.Text, Encoding.GetEncoding("gb2312"));
            };
        }

        private void btnChangeHead_Click(object sender, EventArgs e)
        {
			if (string.IsNullOrEmpty(this.txtContent.Text))
			{
				this.txtContent.Focus();
			}
			else
			{
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				List<string> list = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories).ToList<string>();
				list.RemoveAll((string b) => !b.ToLower().EndsWith(".cs"));
				List<string> list2 = this.txtContent.Text.Split(new char[]
				{
					'\r',
					'\n'
				}, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
				for (int i = 0; i < list2.Count; i++)
				{
					list2[i] = "* " + list2[i];
				}
				string item = "/**";
				list2.Insert(0, item);
				list2.Add("*/");
				int num = 0;
				foreach (string path in list)
				{
					List<string> list3 = File.ReadLines(path).ToList<string>();
					int num2 = -1;
					for (int i = 0; i < list3.Count; i++)
					{
						string text = list3[i];
						if (!text.StartsWith("/*") && !text.StartsWith("*") && !string.IsNullOrEmpty(text.Trim()))
						{
							break;
						}
						num2 = i;
					}
					if (num2 > -1)
					{
						for (int i = 0; i <= num2; i++)
						{
							list3.RemoveAt(0);
						}
					}
					List<string> list4 = new List<string>();
					list4.AddRange(list2);
					list4.AddRange(list3);
					File.WriteAllLines(path, list4.ToArray());
					num++;
				}
				MessageBox.Show("修改文件 " + num);
			}
		}

		private void btnChangeName_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.txtName1.Text))
			{
				this.txtName1.Focus();
				return;
			}
			if (string.IsNullOrEmpty(this.txtName2.Text))
			{
				this.txtName2.Focus();
				return;
			}
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			List<string> files = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories).ToList<string>();
			files.RemoveAll((string b) => !b.ToLower().EndsWith(".cs"));

			int num = 0;
			foreach (string path in files)
			{
				var content = File.ReadAllText(path);
				content = System.Text.RegularExpressions.Regex.Replace(content, @"(using|namespace)(\s+)(" + txtName1.Text + ")(.*)", "$1 " + txtName2.Text + "$4");
				File.WriteAllText(path, content);
				num++;
			}
			MessageBox.Show("修改文件 " + num);


		}
    }
}
