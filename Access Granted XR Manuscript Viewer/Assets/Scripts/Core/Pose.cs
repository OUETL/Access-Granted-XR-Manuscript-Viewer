using UnityEngine;

namespace OU.OVAL.Core
{
    //
    // Have this as a class, rather than a struct, so we can use references. Explicit
    // copy constructor present, so we can explicitly clone another pose if needed.
    //
    // Vector3 and Quaternion are value types (i.e. struct), so can copy them easily.
    //
    public class Pose
    {
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Quaternion rotation;

        public Pose() { }
        public Pose(Vector3 p, Vector3 s, Quaternion r) { Set(p, s, r); }
        public Pose(GameObject go) { Set(go); }
        public Pose(Pose p) { Set(p); }

        public void Set(Vector3 p, Vector3 s, Quaternion r)
        {
            position = p;
            scale = s;
            rotation = r;
        }
        public void Set(GameObject go)
        {
            var t = go.transform;
            Set(t.localPosition, t.localScale, t.localRotation);
        }
        public void Set(Pose p)
        {
            Set(p.position, p.scale, p.rotation);
        }

        public static bool operator != (Pose a, Pose b)
        {
            return (a.position != b.position)
                || (a.scale    != b.scale)
                || (a.rotation != b.rotation);
        }
        public static bool operator == (Pose a, Pose b) { return !(a != b); }

        //
        // The following prevent compiler warnings re. implementing the "==" & "!="
        // operators, but not implementing Equals() or GetHashCode().
        //
        public override bool Equals(object o) { return (this == (Pose)o); }
        public override int  GetHashCode()
        {
            var t = new System.Tuple<Vector3, Vector3, Quaternion>(position, scale, rotation);
            return t.GetHashCode();
        }
    }

    //
    // Easy interpolation between poses
    //
    class PoseUpdate
    {
        Pose prev = new Pose();
        Pose trgt = new Pose();
        float start, duration;

        public Pose current = new Pose();

        public void Reset(GameObject go)
        {
            prev.Set(go);
            trgt.Set(prev);
            current.Set(prev);
        }
        public void Start(GameObject go, Pose target, float dur)
        {
            start = Time.time;
            duration = dur;

            prev.Set(go);
            trgt.Set(target);
            current.Set(prev);
        }
        public bool Update(float time)
        {
            if (prev == trgt) return false; // false == no pose modifications required

            float x = (time - start) / duration;

            if (x > 1f)
            {
                prev.Set(trgt);
                current.Set(trgt);
            }
            else
            {
                current.position = Vector3.Lerp(prev.position, trgt.position, x);
                current.scale = Vector3.Lerp(prev.scale, trgt.scale, x);
                current.rotation = Quaternion.Lerp(prev.rotation, trgt.rotation, x);
            }

            return true; // true == pose modifications required
        }
    }

}
