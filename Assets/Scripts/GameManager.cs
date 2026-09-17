using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    InputController inputController;
    [Header("Performance")]
    [SerializeField] private bool enableVSync = false;
    [SerializeField] private int targetFrameRate = 60;

    [Header("Audio")]
    [SerializeField] private SoundData levelMusic;

    void Awake()
    {
        inputController = GetComponent<InputController>();

        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        Application.targetFrameRate = targetFrameRate;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (levelMusic != null)
        {
            // Fades in smoothly over 1.5 seconds and loops:
            AudioManager.Instance.PlayMusic(levelMusic, fadeDuration: 1.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LockCursor(bool lockCursor = true)
    {
        Cursor.visible = !lockCursor;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
}
