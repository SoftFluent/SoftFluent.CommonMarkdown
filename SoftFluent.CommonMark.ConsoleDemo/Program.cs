using System;
using SoftFluent.CommonMarkdown;

namespace SoftFluent.CommonMark.ConsoleDemo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine(Markdown.ToHtml("# **test**"));
        }
    }
}
