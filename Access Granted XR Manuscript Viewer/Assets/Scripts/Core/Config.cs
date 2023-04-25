using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL.Core
{
    //
    // OVAL configuration options; read from file.
    //
    public class Config
    {
        //
        // Static constants
        //

        static readonly string wordDelim = " ";
        static readonly string stringDelim = "\"";
        static readonly char pathSeparatorChar = System.IO.Path.DirectorySeparatorChar;
        static readonly string defaultConfigPath = "OVAL.config.txt";

        //
        // Instance variables
        //

        public string networkRegion = "us";  // ensure clients use a common region in PUN
        public string userName = "";         // defaults to username on current computer
        public string modelPath = "";        // defaults to directory the OVAL program was run from
        public string screenshotPath = "";   // default to Unity's output directory

        public List<string> defaultPanels = null; // default panels to show on startup

        public float visualScaleFactor = 1.0f; // accessibility - enable scaling of UI objects etc
        public float motionScaleFactor = 1.0f; // accessibility - enable control of motion rates

        public Vector3 vrOffset = Vector3.zero; // offset of starting VR camera
        public double limitDisplayedSize = 0.0; // limit size (on any dimension) of OVALObjects. <= 0? Ignore.

        public Dictionary<string,string> rooms = new Dictionary<string,string>();
        public Dictionary<string,Color> colors = new Dictionary<string, Color>();

        public Config()
        {
            userName = System.Environment.UserName;
            modelPath = System.Environment.CurrentDirectory;

            if( System.IO.File.Exists(defaultConfigPath) )
            {
                ReadFromFile(defaultConfigPath);
            }
            else
            {
                //Debug.Log( "Default config file '" + defaultConfigPath + "' not present." );
            }
        }

        // Use first token on non-comment lines to pass information onto parsing routines.
        public bool ReadFromFile(string filePath)
        {
            var parsers = new Dictionary<string, System.Func<List<string>, bool>>() {
                {"rooms",             ParseRooms},
                {"color",             ParseColor},
                {"username",          ParseUserName},
                {"modelpath",         ParseModelPath},
                {"screenshotpath",    ParseScreenshotPath},
                {"vroffset",          ParseVROffset},
                {"limitdisplayedsize",ParseLimitDisplayedSize},
                {"networkregion",     ParseNetworkRegion},
                {"defaultpanels",     ParseDefaultPanels},
                {"visualscalefactor", ParseVisualScaleFactor},
                {"motionscalefactor", ParseMotionScaleFactor},
            };

            try
            {
                var lines = System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                for (int i = 0; i < lines.Length; i++)
                {
                    var tokens = MiscUtil.Tokenize(lines[i], wordDelim, stringDelim);
                    if (tokens.Count < 1 || tokens[0][0] == '#') continue;

                    var key = tokens[0].ToLower();

                    if (!parsers.ContainsKey(key))
                    {
                        Debug.LogWarning($"No parser for '{key}'");
                    }

                    if (!parsers[key](tokens))
                    {
                        Debug.LogWarning($"Unable to parse '{lines[i]}'");
                    }

                    if (!parsers.ContainsKey(key) || !parsers[key](tokens))
                    {
                        Debug.LogWarning("Ignoring malformed line " + (i+1) + " '" + lines[i] + "'");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Config::ReadFromFile() : " + e);
                return false;
            }
            return true;
        }

        //
        // File parsing routines follow.
        //

        // Check if tokens in "toks" represent a plausible candidate for format string "s"
        // by checking first token in both (case insensitive) and that sufficient tokens exist
        // in "s" to cover all tokens in "toks".
        bool Check( string s, List<string> toks )
        {
            var chk = MiscUtil.Tokenize(s, wordDelim, stringDelim );
            if (toks.Count < chk.Count) return false;
            if ( (toks.Count>0) && (toks[0].ToLower()!=chk[0].ToLower()) ) return false;
            return true;
        }

        bool ParseRooms( List<string> tokens )
        {
            if (!Check("Rooms",tokens)) return false; // We technically allow an empty "Rooms" line.
            for (int i = 1; i < tokens.Count; i++) { rooms[tokens[i]] = ""; }
            return true;
        }

        bool ParseColor(List<string> tokens)
        {
            if (!Check("Color <name> <r> <g> <b>", tokens)) return false; // no "a" in check string, as alpha parameter is optional.
            var c = new Color
            {
                r = Mathf.Clamp(float.Parse(tokens[2]), 0f, 1f),
                g = Mathf.Clamp(float.Parse(tokens[3]), 0f, 1f),
                b = Mathf.Clamp(float.Parse(tokens[4]), 0f, 1f),
                a = (tokens.Count > 5) ? Mathf.Clamp(float.Parse(tokens[5]), 0f, 1f) : 1f
            };
            colors[tokens[1]] = c;
            return true;
        }

        bool ParseUserName(List<string> tokens)
        {
            if (!Check("UserName <x>", tokens)) return false;
            if (tokens.Count < 2) return false;
            userName = tokens[1];
            return true;
        }

        bool ParseModelPath(List<string> tokens)
        {
            var sepChar = System.IO.Path.DirectorySeparatorChar;
            if (!Check("ModelPath <x>", tokens)) return false;
            modelPath = tokens[1];
            if (!modelPath.EndsWith($"{sepChar}")) modelPath = modelPath + sepChar;
            return true;
        }

        bool ParseScreenshotPath(List<string> tokens)
        {
            if (!Check("ScreenshotPath <x>", tokens)) return false;
            screenshotPath = tokens[1];
            if (!screenshotPath.EndsWith($"{pathSeparatorChar}")) screenshotPath = screenshotPath + pathSeparatorChar;
            return true;
        }

        bool ParseVROffset(List<string> tokens)
        {
            if (!Check("VROffset <dx> <dy> <dz>", tokens)) return false;
            vrOffset = new Vector3()
            {
                x = float.Parse(tokens[1]),
                y = float.Parse(tokens[2]),
                z = float.Parse(tokens[3])
            };
            return true;
        }
        bool ParseLimitDisplayedSize(List<string> tokens)
        {
            if (!Check("LimitDisplayedSize <x>", tokens)) return false;
            limitDisplayedSize = double.Parse(tokens[1]);
            return true;
        }
        bool ParseNetworkRegion(List<string> tokens)
        {
            if (!Check("NetworkRegion <x>", tokens)) return false;
            networkRegion = tokens[1];
            return true;
        }
        bool ParseDefaultPanels(List<string> tokens)
        {
            if (!Check("DefaultPanels", tokens)) return false; // ALL parameters optional; can have e.g. blank line for no default UI
            defaultPanels = new List<string>();
            for (int i = 1; i < tokens.Count; i++) defaultPanels.Add( tokens[i] );
            return true;
        }

        bool ParseVisualScaleFactor(List<string> tokens)
        {
            if (!Check("VisualScaleFactor <x>", tokens)) return false;
            visualScaleFactor = Mathf.Clamp(float.Parse(tokens[1]), 1.0f, 3.0f);
            return true;
        }

        bool ParseMotionScaleFactor(List<string> tokens)
        {
            if (!Check("MotionScaleFactor <x>", tokens)) return false;
            motionScaleFactor = Mathf.Clamp(float.Parse(tokens[1]), 0.25f, 3.0f);
            return true;
        }
    }

}
