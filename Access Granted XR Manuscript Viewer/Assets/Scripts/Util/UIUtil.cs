using System;

using UnityEngine;
using UnityEngine.UI;

/*
 * Misc. UI utility routines
 */

using FormatMap = System.Collections.Generic.Dictionary<string, string>;
using FormatEntry = System.Collections.Generic.KeyValuePair<string, string>;

namespace OU.OVAL
{
    public class UIUtil
    {
        public static GameObject UIRect(string name, Vector2 centreOffset, Vector2 dims, GameObject parent)
        {
            int UILayer = LayerMask.NameToLayer("UI"); // typically 5; values above 8 unassigned, so open to user.

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform);
            go.layer = UILayer;
            go.transform.localPosition = new Vector3( centreOffset.x, centreOffset.y, 0f);
            go.GetComponent<RectTransform>().sizeDelta = dims;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;

            return go;
        }

        public static GameObject UITextBox(string name, string txt, Vector2 centreOffset, Vector2 dims, GameObject parent, FormatMap fm = null)
        {
            var go = UIRect(name, centreOffset, dims, parent);

            // Add default image for background
            go.AddComponent<Image>();

            // Cannot add both Text and Image to same object; create "child" text.
            {
                var child = UIUtil.UIRect("Text", Vector2.zero, dims, go);
                var child_txt = child.AddComponent<Text>();
                child_txt.alignment = TextAnchor.MiddleCenter;
                child_txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                child_txt.text = FormatRTFText(txt, fm);
            }

            return go;
        }

        public static string ColorToRTFColorString( Color c )
        {
            Func<int,int,int, int> Clamp = (x,min,max) => { return (x<min) ? min : System.Math.Min(x,max); };
            int r = Clamp( (int)(c.r*255), 0, 255 );
            int g = Clamp( (int)(c.g*255), 0, 255 );
            int b = Clamp( (int)(c.b*255), 0, 255 );
            int a = Clamp( (int)(c.a*255), 0, 255 );
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
        }

        public static string FormatRTFText(string txt, FormatMap fm = null)
        {
            if (fm == null) return txt;

            string open = "", close = "";

            foreach (FormatEntry e in fm)
            {
                open += string.Format("<{0}{1}{2}>", e.Key, (e.Value == "") ? ("") : ("="), e.Value);
                close = string.Format("</{0}>", e.Key) + close;
            }

            return string.Format("{0}{1}{2}", open, txt, close);
        }

        //
        // Handy derived classes
        //

        public class TextBox
        {
            public FormatMap tags;
            public string rawText;
            public GameObject go;

            public TextBox(string name, string txt, Vector2 centreOffset, Vector2 dims, GameObject parent, FormatMap fm = null)
            {
                rawText = txt;
                tags = (fm == null) ? (new FormatMap()) : (new FormatMap(fm));
                go = UITextBox(name, rawText, centreOffset, dims, parent, fm);
            }

            public void SetText(string txt, TextAnchor anchor = TextAnchor.MiddleCenter)
            {
                Text t = go.GetComponentInChildren<Text>();
                rawText = txt;
                t.text = UIUtil.FormatRTFText(rawText, tags);
                t.alignment = anchor;
            }

            public void SetAlignment(TextAnchor anchor = TextAnchor.MiddleCenter)
            {
                Text t = go.GetComponentInChildren<Text>();
                t.alignment = anchor;
            }

            public void SetPosition( float x, float y, float w, float h )
            {
                Text t = go.GetComponentInChildren<Text>();

                var rt = go.GetComponent<RectTransform>();
                rt.transform.localPosition = new Vector3( x, y, 0f );
                rt.sizeDelta = new Vector2(w,h);

                rt = t.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(w, h); // no need to change localPosition for Text element
            }
        }

        public class Button : TextBox
        {
            public Button(string txt, Vector2 centreOffset, Vector2 dims, GameObject parent, Action<Button> action, FormatMap fm = null) : base(txt, txt, centreOffset, dims, parent, fm)
            {
                go.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { action(this); });
            }
        }
    }
}
