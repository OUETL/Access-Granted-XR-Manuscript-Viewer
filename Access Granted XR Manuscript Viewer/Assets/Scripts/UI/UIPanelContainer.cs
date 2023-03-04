using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OU.OVAL
{
    using EventType = Core.Events.Type;
    using EventArgs = Core.Events.Args;

    //
    // Simply passes on the specified offset/layout info, and adds any child UIPanels.
    //
    public class UIPanelContainer : MonoBehaviour
    {
        public UIPanel.Layout layout = UIPanel.Layout.Down;

        public void Awake()
        {
            Core.Common.Instance.scalableVisualElements.Register( gameObject );
        }

        public void Start()
        {
            UIPanel.SetLayout(layout);

            if (Core.Common.Instance.config.defaultPanels != null)
            {
                //
                // If we specify which panels to display in the config file etc...
                //

                // Deactivate existing panel children
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
                    if (!child) continue;

                    var uip = child.gameObject.GetComponent<UIPanel>();
                    if (!uip) continue;

                    child.gameObject.SetActive(false);
                }

                // Activate in specified order
                foreach (var panelName in Core.Common.Instance.config.defaultPanels)
                {
                    var p = Core.Common.Instance.uiPanels.transform.Find(panelName);
                    if (p) p.gameObject.SetActive(true);
                    else Debug.LogWarning($"UIPanelContainer.Start() : unable to find default panel '{panelName}'; ignoring.");
                }
            }
            else
            {
                //
                // If not specific panels in config etc, use whatever child panels of this game object
                // that are currently visible.
                //

                UIPanel.PopulateUsingChildrenOf(gameObject);
            }

            Core.Events.AddEventHandler(EventType.Network_JoinedRoom, OnJoinedRoom);
            Core.Events.AddEventHandler(EventType.Network_LeftRoom, OnLeftRoom);
        }

        List<GameObject> panelStore = new List<GameObject>();

        private void OnJoinedRoom(object sender, EventArgs args)
        {
            var notMaster = false;
           // var notMaster = !Core.Network.IsMaster();
            panelStore = UIPanel.Snapshot(); // snapshot current panel state for restore when we leave the room
            foreach (var go in panelStore)
            {
                var panel = go.GetComponent<UIPanel>();
                if (panel == null) continue;
                if (panel.masterOnly && notMaster) go.SetActive(false);
            }
        }
        private void OnLeftRoom(object sender, EventArgs args)
        {
            UIPanel.Restore(panelStore); // <- restore snapshot we took on room join. NB: not defaultPanels!
        }
    }
}
