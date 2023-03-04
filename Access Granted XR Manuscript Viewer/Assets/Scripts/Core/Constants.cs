using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL.Core
{
    //
    // Place to put all system-wide constants, so we have consistent definitionns.
    //
    public class Constants
    {
        public enum MovementTarget { Player, Model, Light };

        [System.Flags]
        public enum ButtonFlags : uint
        {
            None = 0,
            Trigger = 1,
            Grip = 1 << 1,
            A = 1 << 2,
            B = 1 << 3
        }

        public static readonly string AnnotationContainerName = "OVALAnnotationContainer";

        public static readonly System.Text.Encoding XmlEncoding = System.Text.Encoding.UTF8;
    }
}
