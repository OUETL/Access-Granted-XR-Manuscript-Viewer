using OU.OVAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static InputMonitor;
using static OU.OVAL.Core.Common;

public class MeasureInputActionTest : MonoBehaviour
{ 
    [Tooltip("Width of indication line when measuring")]
    public float width = 0.01f;

    [Tooltip("Is measurement taken on an object's surface, or in free space?")]
    public bool measureOnSurface = true;

    [Tooltip("If surface measurement, how far from surface is indication line drawn?")]
    public float surfaceOffset = 0.02f;

    [Tooltip("Button to create new measurement")]
    public Button newButton;

    [Tooltip("Text to show current measurement etc")]
    public TextMeshProUGUI statusText;

    //
    // Internals
    //
    public InputActionReference LeftDraw;
    public InputActionReference RightDraw;
    public InputActionReference CancelDraw;
    public InputMonitor InputMonitor;
    OU.OVAL.Core.Annotation doodle = new OU.OVAL.Core.Annotation();
    OU.OVAL.Core.AnnotationRender doodleRender = new OU.OVAL.Core.AnnotationRender();
    GameObject measureTarget = null;

    public enum Tracking { None, Left, Right };
    Tracking tracking = Tracking.None;
    bool clickDown = false;
    //bool isMeasuring = false;

    private void Awake()
    {
        enabled = false;
    }

    private void OnDestroy()
    {
        enabled = false;
    }

    public void ToggleMeasuring()
    {
        //Update() will only be called once the button is presssed,
        //and turned off again when the measuring is done
       enabled = !enabled;
        var text = newButton.GetComponentInChildren<TMP_Text>(true);
        text.text = enabled ? "Cancel Measurement" : "New Measurment";
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
        if (CancelDraw.action.triggered)
        {
            ToggleMeasuring();
            return;
        }

        Vector3 collidedAt = Vector3.zero;

        bool lActive = LeftDraw.action.triggered;
        bool rActive = RightDraw.action.triggered;

        bool dropPoint = false;   // drop a new point in the measurement
        bool trackCursor = true;  // connect the last dropped point to the current cursor position

        //
        // Update tracking & click status
        //
        if (lActive || rActive)
        {
            // Very first click in the whole measurement?
            if (tracking == Tracking.None)
            {
                tracking = lActive ? (Tracking.Left) : (Tracking.Right);
            }

            // First frame of a click along the measurement; drop a point.
            if (clickDown == false)
            {
                dropPoint = true;
            }

            clickDown = true;
        }
        else
        {
            clickDown = false;
        }

        //
        // If we're not tracking anything, stop here.
        //
        if (tracking == Tracking.None) return;

        //
        // Check for collisions of pointer and UI / scene objects.
        //
        HandInputInfo inputInfo = InputMonitor.GetHandInput(tracking);
        PointerCollisionInfo collision = inputInfo.PointerCollisionInfo;
        collidedAt = collision.worldPosition + (collision.worldNormal * surfaceOffset);


        GameObject collidedWithOO = collision.collidedWith;
        if (!measureTarget && collidedWithOO) measureTarget = collidedWithOO;

        //
        // Determine whether to join the current cursor position to the last dropped point.
        //
        if (measureOnSurface)
        {
            if (!collidedWithOO) trackCursor = false;
            if (collidedWithOO != measureTarget) trackCursor = false;
        }

        //
        // At this point, we have all the information we need to manipulate the points etc
        //

        var controllerTransform = inputInfo.HandTransform;
        var pos = (measureOnSurface) ? (collidedAt) : (controllerTransform.position);

        if (dropPoint) // && trackCursor)
        {
            doodle.AddPoint(pos);
            if (doodle.sections[0].Count == 1) doodle.AddPoint(pos); // Also add cursor tracking point, if first point in measurement
            //if (newButton && !newButton.interactable) newButton.interactable = true;
            //Debug.LogWarning( $"{doodle.sections.Count} {doodle.sections[0].Count}" );
        }

        int N = doodle.sections[0].Count;
        if (N > 1) // should always be true at this point, but just in case!
        {
            doodle.sections[0][N - 1] = trackCursor ? pos : doodle.sections[0][N - 2];
            UpdateDoodle();
        }
    }

    void UpdateDoodle()
    {
        doodle.width = width;
        doodle.color = Color.black;

        doodleRender.Update(doodle);

        float scaleFactor = 1.0f;
        string unitStr = "";

/*        if (measureTarget)
        {
            var test = measureTarget.GetTextMetadata("units");
            if (test != null)
            {
                var tokens = MiscUtil.Tokenize(test, " ", "\"");
                try
                {
                    scaleFactor = float.Parse(tokens[0]);
                    unitStr = tokens[1];
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Unable to convert metadata for key 'units' ('{test}') : {e}");
                }
            }
        }*/

        double L = GetDoodleLength() * scaleFactor;
        statusText.text = $"{L:g8} {unitStr}";

    }

    double GetDoodleLength()
    {
        double scaleFactor = 1;
        double d = 0;

        // Get scale factor?
        /*           if(measureTarget)
                   {
                       // scaleFactor = ... ?
                   }*/

        var s = (doodle.sections.Count > 0) ? doodle.sections[0] : null;
        if (s == null) return 0.0;

        for (int i = 1; i < s.Count - 1; i++)
        {
            var r1 = s[i - 1];
            var r2 = s[i];

            // Convert to local scale, if present.
            if (measureTarget)
            {
                r1 = measureTarget.transform.InverseTransformPoint(r1);
                r2 = measureTarget.transform.InverseTransformPoint(r2);
            }

            d += Vector3.Magnitude(r2 - r1);
        }

        return d * scaleFactor;
    }

    public void ClearDoodle()
    {
        doodle.Clear();
        doodleRender.Clear();

        tracking = Tracking.None;
        //if (newButton) newButton.interactable = false;
        measureTarget = null;

        UpdateDoodle();


    }
}
