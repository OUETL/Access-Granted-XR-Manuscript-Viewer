using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL
{
    using EventType = Core.Events.Type;
    using EventArgs = Core.Events.Args;

    //
    // Contains snapshot/restore functionality to ensure we can re-display panels in the same order if we
    // hide the panel container. Otherwise, there seems to be no way to preserve order in which
    // OnEnable/OnDisable is called in child panels, so panel order shuffled when container re-enabled.
    //
    // We can't simply put the snapshot/restore in the PanelContainer's OnEnable/OnDisable methods as
    // those messages are only received *after* the OnEnable/OnDisable are called on the child objects!
    // We therefore cannot e.g. get the list of current active panels in the PanelContainer.OnDisable()
    // as the panels have already been removed.
    //
    public class UIPanel : MonoBehaviour
    {
        public enum Layout { Up, Down, Right, Left };

        public bool masterOnly = false; // is the panel only visible to "master" client(s) after room connection?

        RectTransform rt;

        void Awake()     { rt = GetComponent<RectTransform>(); }
        void OnEnable()  { AddPanel(gameObject); }
        void OnDisable() { RemovePanel(gameObject); }

        public Vector2 GetDimensions()
        {
            return (rt != null) ? (rt.sizeDelta) : (Vector2.zero);
        }
        public void SetPosition(float x, float y)
        {
            if (rt != null) rt.localPosition = new Vector2(x, y);
        }

        //
        // Static variables & methods.
        //

        static Layout layout = Layout.Down;
        static List<GameObject> panels = new List<GameObject>();

        //
        // Snapshot & restore; maintains panel ordering.
        //

        static public List<GameObject> Snapshot()
        {
            var panelStore = new List<GameObject>();
            foreach (var p in panels) panelStore.Add(p);
            return panelStore;
        }
        static public void Restore(List<GameObject> panelStore)
        {
            var tmp = new List<GameObject>(); // Can't iterate over panels[] calling SetActive(): panels[] modified during iteration
            foreach (var p in panels) tmp.Add(p);

            foreach (var p in tmp) p?.SetActive(false);
            foreach (var p in panelStore) p?.SetActive(true);

            PerformLayout();
        }

        //
        // Layout routines
        //

        static public void SetLayout(Layout l) { layout = l; }
        static public void PerformLayout()
        {
            var ofs = Vector2.zero;

            for (int i = 0; i < panels.Count; i++)
            {
                var panel = panels[i]?.GetComponent<UIPanel>();
                if (panel == null) continue;

                var dims = panel.GetDimensions();
                var delta = Vector2.zero;

                switch (layout)
                {
                    case Layout.Up: delta.y += dims.y; break;
                    case Layout.Down: delta.y -= dims.y; break;
                    case Layout.Right: delta.x += dims.x; break;
                    case Layout.Left: delta.x -= dims.x; break;
                }

                //
                // Assumption: SetPosition() sets the centre of the panel.
                // To position adjacent panels we offset the second by half the "size"
                // of the first PLUS half the "size" of the second. Therefore, add initial
                // half of current panel size, then reposition the panel. Finally, add
                // second half of current panel size for positioning the following panel.
                // Note: do not ad initial size/2 to "zero panel" before positioning; we
                // want that panel centre at precisely the initial specified offset.
                //

                if (i > 0) { ofs += delta / 2; }
                panel.SetPosition(ofs.x, ofs.y);
                ofs += delta / 2;
            }
        }
        static public void PopulateUsingChildrenOf(GameObject go)
        {
            if (go == null) return;

            panels.Clear();
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                if (child.activeInHierarchy) AddPanel(child, false); // <- only currently visible panels, delayed layout
            }
            PerformLayout();
        }

        //
        // Internal add/remove panel routines; only accessed internally.
        //

        static void AddPanel(GameObject go, bool performLayout = true)
        {
            var panel = go?.GetComponent<UIPanel>();
            if (panel == null || panels.Contains(go)) return;
            panels.Add(go);
            if(performLayout) PerformLayout();
        }
        static void RemovePanel(GameObject go)
        {
            var panel = go?.GetComponent<UIPanel>();
            if (panel == null || !panels.Contains(go)) return;
            panels.Remove(go);
            PerformLayout();
        }
    }

}
