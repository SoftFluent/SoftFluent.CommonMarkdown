using CodeFluent.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SoftFluent.CommonMarkdown
{
    public class Markdown
    {
        // http://standardmarkdown.com/
        // https://github.com/jgm/stmd
        // http://blog.codinghorror.com/standard-flavored-markdown/
        // http://blog.codinghorror.com/standard-markdown-is-now-common-markdown/

        private const string polyfill = @"
if (typeof console === 'undefined' || typeof console.log === 'undefined') {
    console = { };
    console.log = function() { };
}

if (String.prototype.trim === undefined) {
  String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, '');
  };
}
";

        private static string EncodeJavaScriptString(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            sb.Append("\"");
            return sb.ToString();
        }

        /// <summary>
        /// Parse a markdown document and convert it to HTML.
        /// </summary>
        /// <param name="text">The Markdown text to parse.</param>
        /// <returns>The HTML.</returns>
        public static string ToHtml(string text)
        {
            if (text == null)
                return null;

            Exception exception;
            ScriptEngine engine = new ScriptEngine(ScriptEngine.JavaScriptLanguage);

            // Default javaScript engine does not provide some methods => add them
            using (engine.Parse(polyfill))
            {
                // load stmd.js from resources
                string script;
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetName().Name + ".stmd.js"; // https://github.com/jgm/stmd/tree/master/js
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    script = reader.ReadToEnd();
                }

                // Add an helper method
                script += @"
function markdownToHtml(text) {
var reader = new stmd.DocParser();
var writer = new stmd.HtmlRenderer();

var parsed = reader.parse(text);
return writer.render(parsed);
};";
                using (ParsedScript parsed = engine.Parse(script, true, out exception))
                {
                    text = EncodeJavaScriptString(text);
                    object eval = engine.Eval(text);
                    var result = parsed.CallMethod("markdownToHtml", eval);
                    return (string)result;
                }
            }
        }
    }
}