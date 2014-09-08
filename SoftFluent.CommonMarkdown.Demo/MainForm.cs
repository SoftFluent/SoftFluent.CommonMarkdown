using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SoftFluent.CommonMarkdown.Demo
{
    public partial class MainForm : Form
    {
        private static Regex _newLineRegex = new Regex("(?<!\r)\n", RegexOptions.Compiled);

        public MainForm()
        {
            InitializeComponent();
            Convert();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Convert()
        {
            string html = SoftFluent.CommonMarkdown.Markdown.ToHtml(textBoxMarkdown.Text);
            webBrowser.DocumentText = string.Format("<html><head></head><body>{0}</body></html>", html);
            textBoxHtml.Text = _newLineRegex.Replace(html, "\r\n");
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Convert();
        }
    }
}
