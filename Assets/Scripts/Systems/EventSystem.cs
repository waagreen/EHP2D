
using System;
using System.Collections.Generic;

public static class EventsManager
{
    public static readonly Dictionary<Type, Action<GameEvent>> activeEvents = new();
    public static readonly Dictionary<Delegate, Action<GameEvent>> actionsLookup = new();
    
    // T must be a extension of GameEvent
    // Event to observe -> AddListener<JumpEvent>
    // Method that will be fired must have 'JumpEvent' as a parameter -> (OnPlayerJump)
    public static void AddListener<T>(Action<T> evt) where T : GameEvent
    {
        if (actionsLookup.ContainsKey(evt)) return;

        // Adapter function: converts a base GameEvent to T so Action<T> can be stored as Action<GameEvent>
        void action(GameEvent e) => evt((T)e);

        // Mapping original delegate (key) to the wrapped version (value)
        actionsLookup[evt] = action;

        if (activeEvents.TryGetValue(typeof(T), out var existingAction))
        {
            // If an event of that type already exists, subscribe the action to it
            activeEvents[typeof(T)] = existingAction += action;
        }
        else 
        {
            // Otherwise, this will be the first instance of that event type
            activeEvents[typeof(T)] = action;
        }
    }

    public static void RemoveListener<T>(Action<T> evt) where T : GameEvent
    {
        if (actionsLookup.TryGetValue(evt, out var action))
        {
            if (activeEvents.TryGetValue(typeof(T), out var existingAction))
            {
                existingAction -= action;

                if (existingAction == null)
                {
                    // If it was the last action subscribed to the event, remove that event type from the dictionary
                    activeEvents.Remove(typeof(T));
                }
                else
                {
                    // Otherwise, there are actions still subscribed, so update the dictionary
                    activeEvents[typeof(T)] = existingAction;
                }
            }

            // Remove 'Action<T>' from the lookup table
            actionsLookup.Remove(evt);  
        }
    }

    public static void Broadcast(GameEvent evt)
    {
        // If that event type is active, trigger the action with all methods subscribed
        if (activeEvents.TryGetValue(evt.GetType(), out var action))
        {
            action.Invoke(evt);
        }
    }

    public static void ClearAll()
    {
        activeEvents.Clear();
        actionsLookup.Clear();
    }
}
