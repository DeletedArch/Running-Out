using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    InputController inputController;

    [Header("Audio")]
    [SerializeField] private SoundData levelMusic;

    void Awake()
    {
        inputController = GetComponent<InputController>();
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
