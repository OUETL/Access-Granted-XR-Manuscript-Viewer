using System.Collections.Generic;
using UnityEngine;

//
// Accessibility measure; register various scalable visual elements
// (e.g. UI panels, pointers etc) for runtime scaling.
//

namespace OU.OVAL.Core
{
    public class ScalableVisualElements
    {
        Dictionary<GameObject, Vector3> map = new Dictionary<GameObject, Vector3>();

        public void Register(GameObject go)
        {
            map[go] = go.transform.localScale;
        }

        public void SetScaleFactor(float scaleFactor)
        {
            foreach (var kv in map)
            {
                var go = kv.Key;
                var s = kv.Value;

                go.transform.localScale = scaleFactor * s;
            }
        }
    }
}
