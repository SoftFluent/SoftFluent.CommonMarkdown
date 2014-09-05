using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoftFluent.CommonMarkdown.Demo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Convert();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Convert()
        {
            string html = SoftFluent.CommonMarkdown.Markdown.ToHtml(textBoxMarkdown.Text);
            webBrowser.DocumentText = string.Format("<html><head></head><body>{0}</body></html>", html);
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Convert();
        }
    }
}
