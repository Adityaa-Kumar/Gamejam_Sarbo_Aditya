using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("All ghost AI scripts in the scene")]
    public GhostAI[] ghosts;

    [Header("Power Mode Settings")]
    [Tooltip("Warning time before power mode ends (seconds)")]
    [Range(1f, 5f)]
    public float warningTime = 3f;

    private bool isPowerModeActive = false;
    private float powerModeTimer = 0f;
    private float powerModeDuration = 0f;

    public bool IsPowerModeActive => isPowerModeActive;

    void Start()
    {
        // Auto-find all ghosts if not assigned
        if (ghosts == null || ghosts.Length == 0)
        {
            ghosts = FindObjectsOfType<GhostAI>();
        }
    }

    void Update()
    {
        if (isPowerModeActive)
        {
            powerModeTimer -= Time.deltaTime;

            if (powerModeTimer <= 0f)
            {
                DeactivatePowerMode();
            }
            else if (powerModeTimer <= warningTime)
            {
                // Optional: Flash ghosts to warn player
                HandleWarningPhase();
            }
        }
    }

    public void ActivatePowerMode(float duration)
    {
        isPowerModeActive = true;
        powerModeDuration = duration;
        powerModeTimer = duration;

        // Switch all ghosts to flee mode
        foreach (GhostAI ghost in ghosts)
        {
            if (ghost != null && ghost.currentState != GhostAI.GhostState.Eaten)
            {
                ghost.SetState(GhostAI.GhostState.Flee);
            }
        }

        Debug.Log("Power Mode Activated!");
    }

    void DeactivatePowerMode()
    {
        isPowerModeActive = false;
        powerModeTimer = 0f;

        // Switch all ghosts back to chase mode
        foreach (GhostAI ghost in ghosts)
        {
            if (ghost != null && ghost.currentState == GhostAI.GhostState.Flee)
            {
                ghost.SetState(GhostAI.GhostState.Chase);
            }
        }

        Debug.Log("Power Mode Deactivated!");
    }

    void HandleWarningPhase()
    {
        // Optional: Make ghosts flash or change color slightly
        // This warns the player that power mode is about to end
    }

    public float GetRemainingPowerTime()
    {
        return powerModeTimer;
    }
}
