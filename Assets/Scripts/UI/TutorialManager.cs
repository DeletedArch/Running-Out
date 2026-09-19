using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static void EnableTimer()
    {
        var Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null)
        {
            var playerController = Player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                ITimerAccess.SwitchTimer(true);
            }
        }
    }

    public static void DisableTimer()
    {
        var Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null)
        {
            var playerController = Player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                ITimerAccess.SwitchTimer(false);
            }
        }
    }

    public static void RestoreTimer(float amount)
    {
        var Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null)
        {
            var playerController = Player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                ITimerAccess.ModifyTimer(amount);
            }
        }
    }
}
