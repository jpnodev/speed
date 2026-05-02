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
            int height = 280;
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
            Transform reference = player.visuals != null ? player.visuals : player.transform;
            Vector3 localVel = reference != null ? reference.InverseTransformDirection(vel) : vel;
            float horizontalAngle = Mathf.Atan2(localVel.x, localVel.z) * Mathf.Rad2Deg;

            GUILayout.Label($"Local Vel: Fwd {localVel.z:F1} | Right {localVel.x:F1} | Up {localVel.y:F1}");
            GUILayout.Label($"Horiz Angle: {horizontalAngle:F1}°");

            // Sensors
            bool grounded = player.sensors != null && player.sensors.IsGrounded;
            GUILayout.Label($"Grounded: {grounded}");

            if (player.sensors != null)
            {
                GUILayout.Label($"Slope Angle: {player.sensors.CurrentSlopeAngle:F1}°");
                GUILayout.Label($"Slide Start Angle: {player.slideStartSlopeAngle:F1}°");
            }

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
