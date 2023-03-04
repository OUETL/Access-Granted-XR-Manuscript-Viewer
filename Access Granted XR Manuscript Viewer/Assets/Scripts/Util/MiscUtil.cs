using System.Collections.Generic;

using System.IO;
using System.IO.Compression;

namespace OU.OVAL
{
    public static class MiscUtil
    {
        //
        // Simple compression via Zip. Reduces eg XML sizes by around 60%.
        // https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
        //
        static byte[] zipfunc(byte[] bytes, bool unzip)
        {
            byte[] buf = new byte[4096];
            int N;
            var mode = unzip ? CompressionMode.Decompress: CompressionMode.Compress;

            using (var ms_in  = new MemoryStream(bytes))
            using (var ms_out = new MemoryStream())
            {
                using (var gs = new GZipStream(unzip ? ms_in : ms_out, mode))
                {
                    var src = unzip ? (Stream)gs : ms_in;
                    var dst = unzip ? ms_out     : (Stream)gs;
                    while ((N = src.Read(buf, 0, buf.Length)) != 0) dst.Write(buf, 0, N);
                }
                return ms_out.ToArray();
            }
        }
        public static byte[]   Zip(byte[] bytes) { return zipfunc(bytes, false); }
        public static byte[] Unzip(byte[] bytes) { return zipfunc(bytes,  true); }

        // Randomly permute list order
        public static void RandomShuffle<T>(List<T> list, System.Random prng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = prng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        //
        // Templated serialize/deserialize into XML format - via code suggestions on Stack Overflow.
        //

        public static byte[] XmlSerialize<T>(object obj, bool omitXmlDeclaration) where T : class
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException();
            }

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            var settings = new System.Xml.XmlWriterSettings()
            {
                Encoding = Core.Constants.XmlEncoding,
                OmitXmlDeclaration = omitXmlDeclaration
            };

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = System.Xml.XmlWriter.Create(memoryStream,settings))
                {
                    serializer.Serialize(xmlWriter, (T)obj);
                    return memoryStream.ToArray();
                }
            }
        }

        // Useful for passing as delegate that only expects one parameter (e.g. some of Photon's network code)
        public static byte[] XmlSerialize<T>(object obj) where T : class
        {
            return XmlSerialize<T>(obj,false);
        }
        public static T XmlDeserialize<T>(byte[] bytes) where T : class
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                throw new System.InvalidOperationException();
            }

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            // Encoding determined ffrom the XML data itself, but keep this for symmetry with XmlDeserialize()
            var settings = new System.Xml.XmlReaderSettings() { };

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var xmlReader = System.Xml.XmlReader.Create(memoryStream,settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public static byte[] XmlZipSerialize<T>(object obj) where T : class
        {
            var one = XmlSerialize<T>(obj);
            var two = Zip(one);
            //UnityEngine.Debug.LogWarning( $"{one.Length} => {two.Length}" );
            return two; // Zip(XmlSerialize<T>(obj));
        }
        public static T XmlZipDeserialize<T>(byte[] bytes) where T : class
        {
            var one = Unzip(bytes);
            var two = XmlDeserialize<T>(one);
            //UnityEngine.Debug.LogWarning($"{bytes.Length} => {one.Length}");
            return two; // XmlDeserialize<T>(Unzip(bytes));
        }

        // Generic; can swap precise method to e.g. json etc to suit.
        public static string SerializeToString<T>(object obj) where T : class
        {
            var encoding = Core.Constants.XmlEncoding;
            return encoding.GetString(XmlSerialize<T>(obj, false));
        }
        public static T DeserializeFromString<T>(string s) where T : class
        {
            var encoding = Core.Constants.XmlEncoding;
            return XmlDeserialize<T>(encoding.GetBytes(s));
        }

        //
        // Easy delimiter-based tokenization of string that may contain text enclosed in literal delimiters.
        // E.g. "this is \"an example\" with \"literal delimiters\"" => [ "this", "is", "an example", "with", "literal delimiters" ]
        // Traling unterminated strings not returned.
        //

        public static IEnumerable<string> EnumerateTokens(string line, string wordDelim, string stringDelim = null)
        {
            if (line == null || wordDelim == null) yield break;

            var sb = new System.Text.StringBuilder();
            int add_until = -1, j;

            foreach (var c in line)
            {
                bool push_token = false;

                if (add_until != -1) // in string?
                {
                    if (stringDelim[add_until] == c) // string closed?
                    {
                        add_until = -1;
                        push_token = true;
                    }
                    else sb.Append(c);
                }
                else if ( (stringDelim!=null) && ((j=stringDelim.IndexOf(c)) != -1) ) add_until = j;
                else if (wordDelim.IndexOf(c) != -1) push_token = true;
                else sb.Append(c);

                if (push_token && (sb.Length>0))
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
            }

            // Trailing token? Return! If status == InString, there's an unterminated string literal. Ignore!
            if ((sb.Length > 0) && (add_until == -1))
            {
                yield return sb.ToString();
            }
        }
        public static List<string> Tokenize(string line, string wordDelim, string stringDelim = "\"")
        {
            var tokens = new List<string>();
            foreach (var t in EnumerateTokens(line, wordDelim, stringDelim)) tokens.Add(t);
            return tokens;
        }

    }
}
