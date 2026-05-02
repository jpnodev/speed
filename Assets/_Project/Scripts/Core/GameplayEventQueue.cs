using System.Collections.Generic;
using UnityEngine;

namespace Speed.Core
{
    public enum GameplayEventType
    {
        None,
        Landed,
        TookDamage
        // Add more momentary events here as the game expands
    }

    public struct GameplayEvent
    {
        public GameplayEventType Type;
        public float Timestamp;
        public int Priority;
    }

    public class GameplayEventQueue
    {
        private readonly List<GameplayEvent> _events = new List<GameplayEvent>();

        /// <summary>
        /// Adds a new event to the queue. Higher priority events are handled first.
        /// </summary>
        public void AddEvent(GameplayEventType type, int priority = 0)
        {
            _events.Add(new GameplayEvent
            {
                Type = type,
                Timestamp = Time.time,
                Priority = priority
            });

            // Sort by priority (descending), then chronological (oldest first)
            _events.Sort((a, b) =>
            {
                int priorityComparison = b.Priority.CompareTo(a.Priority);
                if (priorityComparison != 0) return priorityComparison;
                return a.Timestamp.CompareTo(b.Timestamp);
            });
        }

        /// <summary>
        /// Checks if a specific event is currently in the queue.
        /// </summary>
        public bool HasEvent(GameplayEventType type)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Type == type) return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to find and consume (remove) the first occurrence of an event type.
        /// </summary>
        public bool TryConsumeEvent(GameplayEventType type)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Type == type)
                {
                    _events.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes events older than a specific duration (to prevent the queue from filling up with stale/unhandled events).
        /// Should be called at the end of the frame/tick.
        /// </summary>
        public void ClearOldEvents(float maxAgeSeconds = 0.1f)
        {
            float currentTime = Time.time;
            _events.RemoveAll(e => (currentTime - e.Timestamp) >= maxAgeSeconds);
        }

        public void ClearAll()
        {
            _events.Clear();
        }
    }
}
