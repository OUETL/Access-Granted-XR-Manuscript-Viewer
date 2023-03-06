using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDrawInteractable : XRBaseInteractable
{
    enum Tracking { None, Left, Right };
    //hand that is drawing currently
    Tracking tracking = Tracking.None;
    //Transform hand;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        //keep track of which hand is drawing
        IXRSelectInteractor hand = args.interactorObject;


        base.OnSelectEntering(args);
    }

    //void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
}
