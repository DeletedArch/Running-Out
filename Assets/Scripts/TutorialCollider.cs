using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private bool enableTimerOnTrigger = false;
    [SerializeField] private bool disableTimerOnTrigger = false;
    [SerializeField] private float restoreTimerAmount = 0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (enableTimerOnTrigger)
            {
                TutorialManager.EnableTimer();
            }
            if (disableTimerOnTrigger)
            {
                TutorialManager.DisableTimer();
            }
            if (restoreTimerAmount > 0f)
            {
                TutorialManager.RestoreTimer(restoreTimerAmount);
            }
            SwitchSceneToMenu();
        }
    }

    void SwitchSceneToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}


