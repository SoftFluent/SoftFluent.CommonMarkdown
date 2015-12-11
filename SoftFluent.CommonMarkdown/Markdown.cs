using System;
using System.IO;
using System.Reflection;
using System.Text;
using CodeFluent.Runtime.Utilities;

namespace SoftFluent.CommonMarkdown
{
    public static class Markdown
    {
        // http://commonmark.org/
        // http://blog.codinghorror.com/standard-flavored-markdown/
        // http://blog.codinghorror.com/standard-markdown-is-now-common-markdown/
        // https://github.com/jgm/stmd
        // https://raw.githubusercontent.com/jgm/commonmark.js/master/dist/commonmark.js

        private static readonly string _javaScriptCode = LoadJavaScriptCode();

        private static string LoadJavaScriptCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function Main(md) {");
            sb.AppendLine(@"
                if(typeof window === 'undefined') {
                    var window = {};
                }");

            // load stmd.js from resources
            var assembly = Assembly.GetExecutingAssembly();
            var stmdResourceName = assembly.GetName().Name + ".commonmark.js";
            using (Stream stream = assembly.GetManifestResourceStream(stmdResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                sb.Append(reader.ReadToEnd());
            }

            sb.AppendLine(@"
                var commonmark = window.commonmark;
                var reader = new commonmark.Parser();
                var writer = new commonmark.HtmlRenderer();
                var parsed = reader.parse(md);
                return writer.render(parsed);");
            sb.AppendLine("}");
            sb.AppendLine("Main");
            return sb.ToString();
        }

        /// <summary>
        /// Parses a markdown document and convert it to HTML.
        /// </summary>
        /// <param name="commonMarkText">The Markdown text to parse.</param>
        /// <returns>The HTML.</returns>
        public static string ToHtml(string commonMarkText)
        {
            if (string.IsNullOrWhiteSpace(commonMarkText))
                return commonMarkText;

            // NOTE: we could re-use the engine to cache the parsed script, etc. but beware to threading-issues
            using (JsRuntime jsRuntime = new JsRuntime())
            {
                JsRuntime.JsContext.Current = jsRuntime.CreateContext();
                JsRuntime.JsValue jsValue = jsRuntime.ParseScript(_javaScriptCode);

                Exception exception;
                JsRuntime.JsValue mainFunction;
                if (jsValue.TryCall(out exception, out mainFunction)) // Get the Main function
                {
                    JsRuntime.JsValue result;
                    if (mainFunction.TryCall(out exception, out result, null, commonMarkText))
                    {
                        return result.Value as string;
                    }
                }
                return null;
            }
        }
    }
}