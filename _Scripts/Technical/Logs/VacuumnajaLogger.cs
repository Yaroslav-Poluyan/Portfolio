using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace _Scripts.Technical.Logs
{
    public class VacuumnajaLogger : MonoBehaviour
    {
        public static VacuumnajaLogger Instance { get; private set; }
        private StreamWriter _logFile;
        [HideInInspector] public bool _isLogging = true;
        private const string Divider = "==========";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (File.Exists("Assets\\VacuumnajaLog.txt"))
            {
                File.Delete("Assets\\VacuumnajaLog.txt");
            }

            _logFile = File.CreateText("Assets\\VacuumnajaLog.txt");
            // Write headers for the table with the same padding as the values
            _logFile.WriteLine(
                $"{"RpcCall",-10} | {"Event Type",-20} | {"Parameters",-50} | Timestamp");
            _logFile.WriteLine(new string('-', 120)); // Line separator
            SetLogging(PlayerPrefs.GetInt("VacuumnajaLoggerEnabled", 1) == 1);
        }

        public void SetLogging(bool state)
        {
            _isLogging = state;
            PlayerPrefs.SetInt("VacuumnajaLoggerEnabled", state ? 1 : 0);
            LogEvent(Divider, 0);
            LogEvent("Logging", 0, new Dictionary<string, string> {{"State", state.ToString()}});
            LogEvent(Divider, 0);
        }

        private void OnDestroy()
        {
            _logFile?.Close();
        }

        private void LogEvent(string eventType, uint rpcNum, IDictionary<string, string> parameters = null)
        {
            if (_logFile == null) return;
            if (!_isLogging && eventType != Divider &&
                eventType != "Logging") return;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var parametersString = parameters != null
                ? string.Join(", ", parameters.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                : string.Empty;

            _logFile.WriteLine(
                $"{rpcNum.ToString(),-10} | {eventType,-20} | {parametersString,-50} | {timestamp}");
        }


        public void TickEvent(uint rpcNum)
        {
            LogEvent("Tick", rpcNum);
        }

        public void LogEvent(uint rpcNum, String eventType, IDictionary<string, string> parameters)
        {
            LogEvent(eventType, rpcNum, parameters);
        }

        public void LogTick(uint rpcNum)
        {
            TickEvent(rpcNum);
        }
    }

    [UnityEditor.CustomEditor(typeof(VacuumnajaLogger))]
    public class VacuumnajaLoggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var logger = (VacuumnajaLogger) target;
            if (GUILayout.Button("Clear Log File"))
            {
                File.Delete("Assets\\VacuumnajaLog.txt");
            }

            if (GUILayout.Button("Open Log File"))
            {
                System.Diagnostics.Process.Start("Assets\\VacuumnajaLog.txt");
            }

            GUILayout.Label(logger._isLogging ? "Logging is enabled" : "Logging is disabled");
            if (GUILayout.Button("Toggle Logging"))
            {
                logger.SetLogging(!logger._isLogging);
                PlayerPrefs.SetInt("VacuumnajaLoggerEnabled", logger._isLogging ? 1 : 0);
            }
        }
    }
}