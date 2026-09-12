using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    InputController inputController;
    void Awake()
    {
        inputController = GetComponent<InputController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
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
