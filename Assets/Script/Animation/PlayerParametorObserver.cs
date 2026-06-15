using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver
{
    private readonly Dictionary<ParametorID, Func<float>> observers = new();

    public PlayerObserver(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        BoxCollider col = player.GetComponent<BoxCollider>();

        Register(
            ParametorID.Speed, () => rb.linearVelocity.magnitude);

        Register(
            ParametorID.ColSize, () => col.size.y * col.transform.lossyScale.y);
    }

    private void Register(
        ParametorID id,
        Func<float> getter)
    {
        observers[id] = getter;
    }

    public float GetParametor(ParametorID id)
    {
        if (observers.TryGetValue(id, out var getter))
        {
            return getter();
        }

        Debug.LogWarning($"Parametor Not Found : {id}");
        return 0f;
    }
}