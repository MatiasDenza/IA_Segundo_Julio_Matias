using System;
using UnityEngine;

// An ALERT carries a snapshot of the last observed position, not a live target.
public static class WolfAlert
{
    public static event Action<Wolf, Vector3> Howled;
    public static event Action HunterUnavailable;
    public static void Howl(Wolf source, Vector3 lastSeenPosition)
    {
        Debug.Log($"{source.name}: AULLIDO / ALERTA en {lastSeenPosition}");
        Howled?.Invoke(source, lastSeenPosition);
    }
    public static void SendHunterUnavailable() { HunterUnavailable?.Invoke(); }
}
