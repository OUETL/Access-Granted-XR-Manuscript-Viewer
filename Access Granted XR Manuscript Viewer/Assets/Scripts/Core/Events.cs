using System.Collections.Generic;

namespace OU.OVAL.Core
{
    using EventHandlerType = System.EventHandler<Events.Args>;
    using EventHandlerMapType = Dictionary<Events.Type, System.EventHandler<Events.Args>>;

    public class Events
    {
        //
        // Event types.
        //
        // Notes:
        //
        // - Request / response events pairs help avoid race conditions. Request
        //   events can be saved to disk to "record" a session, response events
        //   (e.g. SceneModified) don't need saving as they should be auto-raised
        //   after request events received.
        //
        // - Pose: if source guid in event message == current user guid, ignore
        //   (assumes changes to object already visible locally). For replay, emit
        //   event with source guid == System.Guid.Empty.
        //
        public enum Type {
            //
            // Default
            //

            Unknown,

            //
            // Designed for test purposes; send generic string data etc.
            //

            Generic,

            //
            // Network related.
            //

            Network_Error,
            Network_Ping,
            Network_Connected,       Network_Disconnected,
            Network_JoinedRoom,      Network_LeftRoom,
            Network_OtherJoinedRoom, Network_OtherLeftRoom,

            //
            // Events that modify scene contents by add/remove or show/hide scene data
            //

            LoadRequest,
            SetVisibilityRequest,
            AnnotationRequest,

            //
            // Event to indicate that something about the scene has been modified.
            // Sent after events above recieved to trigger eg UI/info display updates.
            //

            SceneModified,

            //
            // Events that introduce transient changes to scene view, but don't alter
            // scene contents by add/remove or show/hide scene data.
            //

            IndicationRequest,
            Pose,

            //
            // At this point, this event exists only to re-enable certain UI elements
            // that were disabled when a load was requested.
            //

            LoadCompleted,
        };

        //
        // Event arguments
        //

        public class Args : System.EventArgs
        {
            public Type eventType;
            public System.Guid guid;

            public string path;
            public string data;

            public Args() { } // parameterless constructor required for eg deserialization

            public Args(
                Type eventType    = Type.Unknown,
                System.Guid? guid = null,
                string       path = null,
                string       data = null )
            {
                this.eventType = eventType;
                this.guid = guid ?? Common.Instance.guid; // System.Guid.Empty;
                this.path = path;
                this.data = data;
            }
        }

        //
        // Generic and specialized event handlers. Note that raising a specialized event also raises
        // the same event through any generic event handlers registered; be careful if you mix & match.
        //
        // Register/deregister event handlers by e.g.:
        //
        // Events.EventHandler += x(); // Generic; x() will receive *all* events
        // Events.EventHandler -= x();
        //
        // Events.AddEventHandler(Event.Type.X, x); // specialized; x() will get X events
        // Events.RemoveEventHandler(Event.Type.X, x);
        //
        // Here, handler delegate has form x( object sender, Args args ) {}
        //

        public static event EventHandlerType EventHandler;
        public static EventHandlerMapType EventHandlers = new EventHandlerMapType();

        public static void AddEventHandler( Type type, EventHandlerType handler )
        {
            if( !EventHandlers.ContainsKey(type) ) { EventHandlers.Add(type, null); }
            EventHandlers[type] += handler;
        }
        public static void RemoveEventHandler(Type type, EventHandlerType handler)
        {
            if (!EventHandlers.ContainsKey(type)) return;
            EventHandlers[type] -= handler;
        }

        //
        // RaiseLocal() emits only to local listeners. Use Core.Network.RaiseGlobal() to send to all users in room
        // (RaiseGlobal() simply calls RaiseLocal() if the system is not connected to the network).
        //

        public static void RaiseLocal(object sender, Args args)
        {
            if( args == null)
            {
                throw new System.ArgumentNullException("args", "RaiseLocal() : bad argument");
            }

            // Raise as specialized event
            if (EventHandlers.ContainsKey(args.eventType))
            {
                EventHandlers[args.eventType]?.Invoke(sender, args);
            }

            // Raise as generic event
            EventHandler?.Invoke(sender, args);
        }
    }
}
