using UnityEngine;
using Speed.Player;

namespace Speed.Core
{
    public class DebugUI : MonoBehaviour
    {
        public PlayerController player;
        public bool showDebug = true;

        private void OnGUI()
        {
            if (!showDebug || player == null) return;

            int width = 300;
            int height = 200;
            Rect rect = new Rect(10, 10, width, height);

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.normal.textColor = Color.white;
            style.fontSize = 14;

            GUILayout.BeginArea(rect, "Player Debug", style);
            GUILayout.Space(20);

            // State
            string stateName = player.StateMachine.CurrentState != null 
                ? player.StateMachine.CurrentState.GetType().Name 
                : "None";
            GUILayout.Label($"State: {stateName}");

            // Velocity
            Vector3 vel = player.rb != null ? player.rb.linearVelocity : Vector3.zero;
            GUILayout.Label($"Velocity: {vel.x:F1}, {vel.y:F1}, {vel.z:F1}");

            // Sensors
            bool grounded = player.sensors != null && player.sensors.IsGrounded;
            GUILayout.Label($"Grounded: {grounded}");

            // Input Buffer
            GUILayout.Label("--- Input Buffer ---");
            if (player.inputBuffer != null)
            {
                var buffer = player.inputBuffer.GetLiveBuffer();
                if (buffer.Count == 0)
                {
                    GUILayout.Label("[Empty]");
                }
                else
                {
                    float time = Time.time;
                    foreach (var input in buffer)
                    {
                        float age = time - input.Timestamp;
                        GUILayout.Label($"- {input.Action} (Age: {age:F2}s)");
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
