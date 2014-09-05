using System;
using System.IO;
using System.Reflection;
using System.Text;
using CodeFluent.Runtime.Utilities;

namespace SoftFluent.CommonMarkdown
{
    public static class Markdown
    {
        // http://standardmarkdown.com/
        // https://github.com/jgm/stmd
        // http://blog.codinghorror.com/standard-flavored-markdown/
        // http://blog.codinghorror.com/standard-markdown-is-now-common-markdown/

        private static readonly string _stdm = LoadStmdJs();
        private static string LoadStmdJs()
        {
            // this is not provided by IE's javascript
            const string polyfill = @"
                if (typeof console === 'undefined' || typeof console.log === 'undefined') {
                    console = { };
                    console.log = function() { };
                }

                if (String.prototype.trim === undefined) {
                  String.prototype.trim = function () {
                    return this.replace(/^\s+|\s+$/g, '');
                  };
                }";

            // add a helper method
            const string method = @"
                function markdownToHtml(text) {
                    var reader = new stmd.DocParser();
                    var writer = new stmd.HtmlRenderer();
                    var parsed = reader.parse(text);
                    return writer.render(parsed);
                    };";

            // load stmd.js from resources
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetName().Name + ".stmd.js"; // https://github.com/jgm/stmd/tree/master/js
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
                return polyfill + reader.ReadToEnd() + method;
        }

        private static readonly string _language = DetermineBestEngine();
        private static string DetermineBestEngine()
        {
            // use IE9+'s chakra engine?
            bool useChakra = ScriptEngine.GetVersion(ScriptEngine.ChakraClsid) != null;
            return useChakra ? ScriptEngine.ChakraClsid : ScriptEngine.JavaScriptLanguage;
        }

        private static string EncodeJavaScriptString(string s)
        {
            if (s == null)
                return null;

            StringBuilder sb = new StringBuilder();
            sb.Append('"');
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
            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Parses a markdown document and convert it to HTML.
        /// </summary>
        /// <param name="text">The Markdown text to parse.</param>
        /// <returns>The HTML.</returns>
        public static string ToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // NOTE: we could re-use the engine to cache the parsed script, etc. but beware to threading-issues
            using (ScriptEngine engine = new ScriptEngine(_language))
            using (ParsedScript parsed = engine.Parse(_stdm))
                return (string)parsed.CallMethod("markdownToHtml", engine.Eval(EncodeJavaScriptString(text)));
        }
    }
}