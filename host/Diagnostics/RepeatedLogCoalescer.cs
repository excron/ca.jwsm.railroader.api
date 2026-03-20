using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ca.Jwsm.Railroader.Api.Host.Diagnostics
{
    internal static class RepeatedLogCoalescer
    {
        private const float SummaryIntervalSeconds = 5f;
        private static readonly Dictionary<string, State> States = new Dictionary<string, State>(StringComparer.Ordinal);

        private sealed class State
        {
            internal string Message = string.Empty;
            internal int SuppressedCount;
            internal float FirstSeenTime;
            internal float LastSeenTime;
            internal bool Active;
            internal bool Warning;
        }

        internal static void Log(string key, string message)
        {
            LogCore(key, message, warning: false);
        }

        internal static void LogWarning(string key, string message)
        {
            LogCore(key, message, warning: true);
        }

        internal static void Flush(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !States.TryGetValue(key, out var state) || !state.Active)
            {
                return;
            }

            EmitSummary(key, state, recovered: true);
            state.Active = false;
            state.SuppressedCount = 0;
        }

        private static void LogCore(string key, string message, bool warning)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Emit(message, warning);
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (!States.TryGetValue(key, out var state))
            {
                state = new State();
                States[key] = state;
            }

            if (!state.Active || !string.Equals(state.Message, message, StringComparison.Ordinal))
            {
                if (state.Active && state.SuppressedCount > 0)
                {
                    EmitSummary(key, state, recovered: false);
                }

                state.Message = message;
                state.SuppressedCount = 0;
                state.FirstSeenTime = now;
                state.LastSeenTime = now;
                state.Active = true;
                state.Warning = warning;
                Emit(message, warning);
                return;
            }

            state.SuppressedCount++;
            state.LastSeenTime = now;
            if ((now - state.FirstSeenTime) < SummaryIntervalSeconds)
            {
                return;
            }

            EmitSummary(key, state, recovered: false);
            state.FirstSeenTime = now;
            state.SuppressedCount = 0;
        }

        private static void EmitSummary(string key, State state, bool recovered)
        {
            if (state.SuppressedCount <= 0)
            {
                return;
            }

            float duration = Mathf.Max(0f, state.LastSeenTime - state.FirstSeenTime);
            string message =
                $"[ca.jwsm.railroader.api.host] Suppressed {state.SuppressedCount} repeated '{key}' log(s) over {duration:0.0}s." +
                (recovered ? " Condition recovered." : string.Empty);
            Emit(message, state.Warning);
        }

        private static void Emit(string message, bool warning)
        {
            if (warning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
    }
}
