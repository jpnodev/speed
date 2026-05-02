using System.Collections.Generic;
using UnityEngine;

namespace Speed.Inputs
{
    public enum PlayerInputAction
    {
        None,
        Jump,
        Dash
        // Add more buffered actions here
    }

    public struct BufferedInput
    {
        public PlayerInputAction Action;
        public float Timestamp;
    }

    public class InputBuffer : MonoBehaviour
    {
        [Header("Settings")]
        public float defaultBufferTime = 0.15f;

        private readonly List<BufferedInput> _buffer = new List<BufferedInput>();

        public void BufferAction(PlayerInputAction action)
        {
            _buffer.Add(new BufferedInput
            {
                Action = action,
                Timestamp = Time.time
            });
        }

        public bool Consume(PlayerInputAction action)
        {
            float currentTime = Time.time;
            for (int i = 0; i < _buffer.Count; i++)
            {
                if (_buffer[i].Action == action)
                {
                    // Check if the input is still valid (not too old)
                    if (currentTime - _buffer[i].Timestamp <= defaultBufferTime)
                    {
                        _buffer.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        private void Update()
        {
            // Clean up stale inputs to prevent phantom actions happening much later
            float currentTime = Time.time;
            _buffer.RemoveAll(input => (currentTime - input.Timestamp) > defaultBufferTime);
        }

        public List<BufferedInput> GetLiveBuffer()
        {
            // Used mainly for DebugUI
            return _buffer;
        }
    }
}
