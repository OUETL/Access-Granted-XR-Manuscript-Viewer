using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL
{
    public static class ObjectUtil
    {
        static readonly string gameobjectPathSeparator = "/"; // as per Unity docs for transform.Find()

        // Fully-resolved Unity game object path in heirarchy
        public static string GetGameObjectPath(GameObject go, GameObject stopAt)
        {
            //string pathSeparator = "/";

            var t = go.transform;
            string path = t.name;
            while ((t.parent != null) && (t.parent.gameObject != stopAt))
            {
                t = t.parent;
                path = t.name + gameobjectPathSeparator + path;
            }
            return path;
        }

        // Game object path relative to the OVAL scene container
        public static string GetGameObjectPath(GameObject go)
        {
            return GetGameObjectPath(go, Core.Common.Instance.sceneContainer);
        }

        public static List<string> GetGameObjectPathTokens( string path )
        {
            return MiscUtil.Tokenize( path, gameobjectPathSeparator );
        }

        // Look for model in sceneContainer. transform.Find() will return inactive objects,
        // whereas GameObject.Find() will not! Hence, use transform find.
        public static GameObject FindModel(string modelPath)
        {
            var t = Core.Common.Instance.sceneContainer.transform.Find(modelPath);
            return t?.gameObject;
        }

        // Find and set active status for model. Returns false if model not found, true otherwise.
        public static bool SetActive(string modelPath, bool isActive)
        {
            var go = FindModel(modelPath);
            if (go == null)
            {
                Debug.LogWarning($"MiscUtil.SetActive(): Can't find object {modelPath}");
                return false;
            }
            go.SetActive(isActive);
            return true;
        }

        public static void GetTransforms(Transform t, List<Transform> all)
        {
            all.Add(t);
            for (int i = 0; i < t.childCount; i++) GetTransforms(t.GetChild(i), all);
        }

        public static void SetShadows(GameObject obj, bool shadows)
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend == null) return;

            rend.receiveShadows = shadows;
            rend.shadowCastingMode = (rend.receiveShadows)
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // OVALObject might be applied to an object with one or more child objects
        // with different local translations/rotations/scales; we should therefore
        // take this into account!
        public static Bounds CalculateWorldBounds(List<Transform> all)
        {
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            for (int i = 0; i < all.Count; i++)
            {
                var mf = all[i].gameObject.GetComponent<MeshFilter>();
                if (mf == null) continue;

                var m = all[i].localToWorldMatrix;
                var world_min = m.MultiplyVector(mf.mesh.bounds.min);
                var world_max = m.MultiplyVector(mf.mesh.bounds.max);

                if (i == 0) { min = world_min; max = world_max; }
                min = Vector3.Min(min, world_min);
                max = Vector3.Max(max, world_max);
            }
            return new Bounds((min + max) / 2, max - min);
        }

    }
}
