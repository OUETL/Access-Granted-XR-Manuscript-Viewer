using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL.Core
{
    using EventType = Events.Type;
    using EventArgs = Events.Args;

    using EventTypeToArgs = SerializableDictionary<Events.Type, Events.Args>;
    using ObjectViewStatus = SerializableDictionary<string, SerializableDictionary<Events.Type, Events.Args>>;

    //
    // Store/restore OVAL scenes via the events used to generate them.
    // Take initial snapshot, then add appropriate events as they fly by.
    //
    public class SceneInfo
    {
        // Sequence of events that changed scene contents (load,annotate,etc)
        public List<EventArgs> contentEvents = new List<EventArgs>();

        // Most recent per-object events that changed "view" (show,hide,pose,etc)
        public ObjectViewStatus viewEvents = new ObjectViewStatus();

        public SceneInfo() { }
        public SceneInfo(SceneInfo si) { Clone(si);  }

        public void Clone( SceneInfo source )
        {
            contentEvents.Clear();
            foreach (var args in source.contentEvents) UpdateContentEvents( args );

            viewEvents.Clear();
            foreach (var kv1 in source.viewEvents)
                foreach (var kv2 in kv1.Value)
                {
                    var args = kv2.Value;
                    UpdateViewEvents(args);
                }
        }

        //
        // Snapshot functionality
        //
        void UpdateViewEvents(EventArgs args) // internal; guarantees only called with valid event type.
        {
            var key = args.path;
            if (key == null)
            {
                Debug.LogWarning($"UpdateViewEvents: Add() with null key ({args.eventType})");
                return;
            }

            EventTypeToArgs e2a;
            if (!viewEvents.TryGetValue(key, out e2a))
            {
                viewEvents[key] = new EventTypeToArgs();
            }
            viewEvents[key][args.eventType] = args;
        }
        void AddSnapshotEvents(GameObject go)
        {
            if (go == null) return;

            var path = ObjectUtil.GetGameObjectPath(go);

            UpdateViewEvents(new EventArgs(
                EventType.SetVisibilityRequest,
                path: path,
                data: go.activeSelf ? "1" : null // NOT go.activeInHierarchy!
                ));

            UpdateViewEvents(new EventArgs(
                EventType.Pose,
                path: path,
                data: MiscUtil.SerializeToString<Pose>(new Pose(go))
                ));
        }
        public void Snapshot(Transform root)
        {
            if (root == null) return;

            List<Transform> all = new List<Transform>();
            ObjectUtil.GetTransforms(root, all);

            foreach (var t in all)
            {
                var go = t.gameObject;
                var oo = go?.GetComponent<OVALObject>();
                if (oo == null) continue;

                AddSnapshotEvents(go);

                var ac = oo.annotationContainer;
                if (ac == null) continue;

                for (int i = 0; i < ac.transform.childCount; i++)
                {
                    go = ac.transform.GetChild(i).gameObject;
                    AddSnapshotEvents(go);
                }
            }
        }

        public void UpdateContentEvents(EventArgs args)
        {
            switch (args.eventType)
            {

                case EventType.LoadRequest:
                    contentEvents.Add(args);
                    break;
            }
        }

        // Raise events to construct a scene consistent with this SceneInfo.
        // NOTE: actual event guid overridden with Guid.Empty (same as
        // recording playback), as otherwise eg Pose events may be ignored!
        public void RaiseSceneEvents( bool raiseContentEvents = true, bool raiseViewEvents = true )
        {
            var playbackGuid = System.Guid.Empty;

            // Events that add/remove scene content raised first
            if(raiseContentEvents)
            {
                foreach (var args in contentEvents)
                {
                    Events.RaiseLocal(this, new EventArgs(
                        eventType: args.eventType,
                        guid: playbackGuid,
                        path: args.path,
                        data: args.data));
                }
            }

            // Events that alter view of scene contents raised after
            if (raiseViewEvents)
            {
                foreach (var kv1 in viewEvents)
                    foreach (var kv2 in kv1.Value)
                    {
                        var args = kv2.Value;
                        Events.RaiseLocal(this, new EventArgs(
                            eventType: args.eventType,
                            guid: playbackGuid,
                            path: args.path,
                            data: args.data));
                    }
            }
        }
    }
}
