using System;
using UnityEngine;

public class PlayerDetectionHandler : MonoBehaviour
{
    private LayerMask playerLayer;
    private GameObject targetPlayer;
    private PlayerSide side;

    public void SetTargetPlayer(GameObject player, PlayerSide side)
    {
        this.targetPlayer = player;
        this.side = side;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetPlayer)
        {
            UEventBus<PlayerGoalDetectionEvent>.Raise(new PlayerGoalDetectionEvent(true, side, targetPlayer));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetPlayer)
        {
            UEventBus<PlayerGoalDetectionEvent>.Raise(new PlayerGoalDetectionEvent(false, side, targetPlayer));
        }
    }
}
