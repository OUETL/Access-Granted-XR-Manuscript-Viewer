using System.Collections.Generic;

using UnityEngine;
//using UnityEngine.UI.Player;
using UnityEngine.UIElements;

namespace OU.OVAL
{
    public class MoveUIPanel : MonoBehaviour
    {
        enum Tracking { None, Left, Right };

        [Tooltip("Which GameObject to move (typically the UI panel container)")]
        public GameObject target;

        public Button moveButton;
        public Toggle visibilityToggle;
        public Slider visualScaleSlider;

        Tracking tracking = Tracking.None;

        Vector3 targetR0, reference;
        float distanceToUI; // initial distance from controller -> UI

        Vector3 initialScale = new Vector3(1,1,1);

        //
        // Subtlety: clicking on the move button works fine, but clicking again to end the drag + enabling the
        // buttons is problematic, as the buttons seem to be enabled in such a way to interpret the second
        // press of the trigger as another click in many circumstances. We therefore use a little state machine
        // to end the drag epoch (and enable the buttons!) only after the trigger is released after the second click.
        //
        // status == 0 ? dragging UI, waiting for second trigger down
        // status == 1 ? dragging UI, waiting for second trigger up
        // status == 2 ? dragging UI and got second complete trigger click, end drag & enable buttons.
        //
        int status = 0;

        // We need two panel stores to restore hidden UI panels after dragging UI in hidden state!
        List<GameObject> panelStore = null;     // for UI-driven show/hide
        List<GameObject> panelStoreDrag = null; // for drag-driven show/hide

        bool lWasDown = false;

        string txtStore;

        string SetButtonText( string t )
        {
            string old = null;
            //var txt = moveButton?.GetComponentInChildren<Text>();
            //var txt = moveButton.text;
            //if (txt) { old = txt.text; txt.text = t; }
            return old;
        }
        List<GameObject> HidePanels()
        {
            var l = UIPanel.Snapshot();
            foreach (var p in l)
            {
                if (p && p != gameObject) p.SetActive(false);
            }
            return l;
        }
        void ShowPanels( List<GameObject> l )
        {
            UIPanel.Restore(l);
        }

        void StartDragEpoch()
        {
            var common = Core.Common.Instance;

            if (tracking != Tracking.None) return;

            //
            // Test for clicks; the UI seems to get the click event BEFORE the Update() routine is
            // called, so we have to check whether the UI button was clicked with the left or right
            // controller here!
            //
            {
/*                var lActive = common.inputWrapper.GetLeft().Pressed(Core.Constants.ButtonFlags.Trigger);
                tracking = (!lActive && lWasDown) ? Tracking.Left : Tracking.Right;*/
            }

            var transform = (tracking == Tracking.Left) ? (common.lTransform) : (common.rTransform);

            //txtStore = SetButtonText("Moving");
            if (moveButton != null) moveButton.SetEnabled(false);
            if (visibilityToggle != null) visibilityToggle.SetEnabled(false);

            targetR0 = target.transform.position;
            distanceToUI = Vector3.Magnitude(transform.position - target.transform.position);
            reference = transform.position + distanceToUI * transform.forward;

            status = 0;
            panelStoreDrag = HidePanels();
        }
        void EndDragEpoch()
        {
            tracking = Tracking.None;
            SetButtonText( txtStore );
            //if (moveButton) moveButton.interactable = true;
            //if(visibilityToggle) visibilityToggle.interactable = true;
            ShowPanels(panelStoreDrag);
        }

        private void Awake()
        {
            //moveButton?.onClick.AddListener( OnMoveClick );
            moveButton.clicked += OnMoveClick;
            // visibilityToggle?.onValueChanged.AddListener( OnVisibilityClick );
           // visibilityToggle.onValueChanged += OnVisibilityClick;
        }

        void Start()
        {
            var visualScaleFactor = Core.Common.Instance.config.visualScaleFactor;

            // Check motion scale factor slider
            if (visualScaleSlider == null) Debug.LogWarning("No visual scale slider specified!");
            else
            {
                if (target != null) initialScale = target.transform.localScale;

/*                visualScaleSlider.onValueChanged.AddListener(delegate {
                    var val = visualScaleSlider.value;
                    Core.Common.Instance.config.visualScaleFactor = val;
                    if (target != null) target.transform.localScale = initialScale * val;
                });*/

                // Should invoke the handler above to set the initial value etc.
                visualScaleSlider.value = visualScaleFactor;
            }
        }

        void Update()
        {
            var common = Core.Common.Instance;
            /*            var lActive = common.inputWrapper.GetLeft().Pressed(Core.Constants.ButtonFlags.Trigger);
                        var rActive = common.inputWrapper.GetRight().Pressed(Core.Constants.ButtonFlags.Trigger);*/

            var lActive = true;
            var rActive = true;
            // For detection of which trigger was pressed on previous frame and then released
            // in StartDragEpoch(); used to determine which controller to track.
            lWasDown = lActive;

            if (tracking == Tracking.None) return;

            var transform = (tracking == Tracking.Left) ? (common.lTransform) : (common.rTransform);

            var controllerQ1 = transform.rotation;
            var controllerR1 = transform.position;

            var active = (tracking == Tracking.Left) ? lActive : rActive;

            if (status == 0 && active) // dragging & second trigger press?
            {
                status++;
            }
            else if (status == 1 && !active) // dragging & second trigger release?
            {
                EndDragEpoch();
                return;
            }

            //
            // x*M0 = M1 : x is matrix taking us from initial matrix (M0) to current (M1)
            // x*M0*M0^{-1} = M1*M0^{-1}
            // x = M1*M0^{-1}
            //
            /*
            var cM0 = Matrix4x4.Rotate(controllerQ0);
            var cM1 = Matrix4x4.Rotate(controllerQ1);
            var x = cM1 * Matrix4x4.Inverse(cM0);

            var tM0 = Matrix4x4.Rotate(targetQ0);
            var tM1 = tM0 * x;
            target.transform.rotation = Quaternion.LookRotation(tM1.GetColumn(2), tM1.GetColumn(1));
            */

            //
            // Fix positioning of UI the face player on the surface of a cylider.
            //
            var dr = Vector3.Normalize(target.transform.position - transform.position);
            target.transform.rotation = Quaternion.LookRotation( new Vector3(dr.x,0,dr.z), new Vector3(0,1,0) );

            //
            // newpos = oldpos + (newBeam - oldBEam), where oldBeam = original collision point with beam and model, newBeam = location of new beam out to same distance as old collision
            //
            var delta = (transform.position + distanceToUI * transform.forward) - reference;
            target.transform.position = targetR0 + delta;
        }

        void OnMoveClick() { StartDragEpoch(); }
        void OnVisibilityClick( bool b )
        {
            if (b) ShowPanels(panelStore);
            else panelStore = HidePanels();
        }
    }

}