using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL.Core
{
    using TimestampedEvent = System.Tuple<float, Events.Args>;

    class SceneRecording
    {
        float startTime = -1f;
        List<TimestampedEvent> recording = new List<TimestampedEvent>();

        void OnEvent(object raisedBy, Events.Args args)
        {
            var timestamp = Time.time - startTime;
            recording.Add(new TimestampedEvent(timestamp, args));
        }

        public void StartRecording(SceneInfo si)
        {
            recording.Clear();
            startTime = Time.time;

            foreach (var args in si.contentEvents)
            {
                recording.Add(new TimestampedEvent(0, args));
            }

            foreach (var kv1 in si.viewEvents)
                foreach (var kv2 in kv1.Value)
                {
                    var args = kv2.Value;
                    recording.Add(new TimestampedEvent(0, args));
                }

            Events.EventHandler += OnEvent; // i.e., ALL events
        }
        public void StopRecording()
        {
            Events.EventHandler -= OnEvent;
        }

        // Retrieves current recorded events and clear the recording;
        // allows periodic writes to disk and limits memory overheads.
        public List<TimestampedEvent> FlushRecordedEvents()
        {
            var tmp = recording;
            recording = new List<TimestampedEvent>();
            return tmp;
        }
    }
}
