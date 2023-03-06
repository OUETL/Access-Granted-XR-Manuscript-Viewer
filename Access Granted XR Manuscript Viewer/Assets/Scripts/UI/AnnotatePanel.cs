using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using ColorOption = System.Collections.Generic.KeyValuePair<string, UnityEngine.Color>;
using FormatMap = System.Collections.Generic.Dictionary<string, string>;

namespace OU.OVAL
{
    using EventType = Core.Events.Type;
    using EventArgs = Core.Events.Args;

    public class AnnotatePanel : MonoBehaviour
    {
        [Tooltip("Limit rate at which annotation points are created")]
        public int maxRate = 30; // max. deposit rate, in points per second

        [Tooltip("Only drop new annotation points if at least this far from last point")]
        public float motionThreshold = 0.01f; // can't rely on this alone; natural wobble in hands can exceed this for distant targets!

        [Tooltip("Should we draw annotations onto the surface of an object?")]
        public bool drawOnSurface = true;

        [Tooltip("If annotation on object surface, how far from the surface is the annotation drawn?")]
        public float surfaceOffset = 0.02f;

        public Dropdown colorDropdown = null;
        public Button clearButton = null;
        public Button storeButton = null;
        public Slider widthSlider = null;

        //
        // Internals
        //

        GameObject doodleTarget = null;
        Core.Annotation doodle = new Core.Annotation();
        Core.AnnotationRender doodleRender = new Core.AnnotationRender();
        int doodleID = 1;

        List<ColorOption> colorOptions;

        enum Tracking { None, Left, Right };
        Tracking tracking = Tracking.None;

        float lastPointTime; // time of last point deposition; for rate limiting

        void ClearDoodle()
        {
            doodle.Clear();
            doodleRender.Clear();

            doodleTarget = null;

            UpdateDoodle();
            clearButton.interactable = false;
            storeButton.interactable = false;
        }
        void UpdateDoodle() // set width directly via lr.widthMultipler when slider moved? faster than full rebuild?
        {
            int i = colorDropdown?.value ?? 0;

            var width = widthSlider?.value ?? 0.1f;
            var c = colorOptions[i].Value;

            doodle.width = width;
            doodle.color = c;

            doodleRender.Update(doodle);
        }

        private void Awake()
        {
            //
            // Check UI elements, add handlers.
            //
            if (!colorDropdown) Debug.LogWarning("No color dropdown specified!");
            colorDropdown?.onValueChanged.AddListener(delegate (int x) { UpdateDoodle(); });

            if (!widthSlider) Debug.LogWarning("No width slider specified!");
            widthSlider?.onValueChanged.AddListener(delegate (float x) { UpdateDoodle(); });

            if (!clearButton) Debug.LogWarning("No clear button defined!");
            clearButton?.onClick.AddListener(delegate { ClearDoodle(); });

            if (!storeButton) Debug.LogWarning("No store button defined!");
            storeButton?.onClick.AddListener(delegate { StoreDoodle(); });

            //
            // Set up color options; defaults and any user-defined from Common.
            //
            colorOptions = new List<ColorOption>()
            {
                new ColorOption("Black", Color.black),
                new ColorOption("Blue", Color.blue),
                new ColorOption("Green", Color.green),
                new ColorOption("Red", Color.red),
                new ColorOption("White", Color.white),
                new ColorOption("Yellow", Color.yellow)
            };
            foreach (var c in Core.Common.Instance.config.colors) colorOptions.Add(c);

            if (colorDropdown)
            {
                FormatMap fm = new FormatMap();
                List<string> options = new List<string>();

                foreach (var c in colorOptions)
                {
                    fm["color"] = UIUtil.ColorToRTFColorString(c.Value);
                    options.Add(UIUtil.FormatRTFText("\u2588", fm) + " " + c.Key);
                }

                colorDropdown.ClearOptions();
                colorDropdown.AddOptions(options);
            }
        }

        private void OnEnable()
        {
            ClearDoodle();
        }
        private void OnDisable()
        {
            ClearDoodle();
        }

        void Update()
        {
            Vector3 collidedAt = Vector3.zero;

            var common = Core.Common.Instance;
            /*            var lActive = common.inputWrapper.GetLeft().Pressed(Core.Constants.ButtonFlags.Trigger);
                        var rActive = common.inputWrapper.GetRight().Pressed(Core.Constants.ButtonFlags.Trigger);*/

            var lActive = false;
            var rActive = false;

            // If we were tracking but have now stopped...
            if (!lActive && !rActive)
            {
                tracking = Tracking.None;
                return;
            }

            bool firstTrackingFrame = false;

            // If we are on first tracking frame, determine which controller to track,
            // else test rate limit. Note: we can't clear the current doodle here, in
            // preparation for a new doodle, as as we don't yet know if user hits this
            // code as part of e.g. using UI elements! Set "shouldClearDoodle" instead,
            // and we can deal with it later if needed.
            if (tracking == Tracking.None)
            {
                firstTrackingFrame = true;
                tracking = (lActive) ? (Tracking.Left) : (Tracking.Right);
                lastPointTime = Time.time;
            }
            else
            {
                if ((Time.time - lastPointTime) < (1.0 / maxRate)) return;
            }

            lastPointTime = Time.time;
            var controllerTransform = (tracking == Tracking.Left) ? (common.lTransform) : (common.rTransform);

            // Determine what our beam is hitting, if anything. We need to test for UI hits regardless
            // of whether we're drawing on an object's surface or in free space.
            {
                var collision = (tracking == Tracking.Left) ? (common.lCollision) : (common.rCollision);
                var obj = collision.collidedWith;

                if (obj != null)
                {
                    if (obj.layer == LayerMask.NameToLayer("UI")) return;
                    else
                    {
                        doodleTarget = obj;
                        collidedAt = collision.worldPosition + (collision.worldNormal * surfaceOffset);
                    }
                }
                else
                {
                    if (drawOnSurface) return;
                }
            }

            //
            // At this point, we're in a valid state and can deposit points!
            //

            if (firstTrackingFrame) doodle.AddSection();
            firstTrackingFrame = false;

            var pos = (drawOnSurface) ? (collidedAt) : (controllerTransform.position);

            //
            // Disqualify new point if it is too close to last point.
            //
            int Ns = doodle.sections.Count;
            var s = (Ns > 0) ? doodle.sections[Ns - 1] : null;
            if ((s!=null) && (s.Count>0))
            {
                var dr = s[s.Count-1] - pos;
                if (dr.sqrMagnitude < (motionThreshold * motionThreshold)) return;
            }

            doodle.AddPoint(pos);

            //
            // If we have more than one point, update UI and displayed annotation
            //
            if ((s!=null) && (s.Count>1))
            {
                if (!clearButton.interactable) clearButton.interactable = true;
                if (!storeButton.interactable) storeButton.interactable = true;
                UpdateDoodle();
            }
        }

        public void StoreDoodle()
        {
            int Ns = doodle.sections.Count;
            var s = (Ns > 0) ? doodle.sections[Ns - 1] : null;
            if ((s!=null) && (s.Count<2)) return;

            // Hack. We need a deterministic way to identify arbitrary objects, so show/hide etc
            // will work properly across the network even if mulitple objects with same name!
            doodle.name = $"Doodle{doodleID}";
            doodleID++;

            var path = ObjectUtil.GetGameObjectPath(doodleTarget);
            var data = MiscUtil.SerializeToString<Core.Annotation>(doodle);
            //Core.Network.RaiseGlobal(this, new EventArgs(EventType.AnnotationRequest, path: path, data: data));

            ClearDoodle();
        }
    }
}