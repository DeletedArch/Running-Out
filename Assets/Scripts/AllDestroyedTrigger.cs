using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AllDestroyedTrigger : MonoBehaviour
{
    [Header("Objects to track")]
    [Tooltip("Drag the GameObjects to monitor here. Once all of them are destroyed, the event will fire.")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();

    [Header("Action on All Destroyed")]
    [Tooltip("Actions to trigger in the Inspector once all tracked objects are destroyed.")]
    [SerializeField] private UnityEvent onAllDestroyed;

    [Header("UI Feedback")]
    [SerializeField] private Text lose;
    [SerializeField] private Text win;
    [SerializeField] private float restartDelay = 1.5f;

    public event Action OnAllDestroyed;

    private bool isTracking = false;
    private bool hasTriggered = false;
    private bool isGameOver = false;
    private bool isWon = false;

    void OnEnable()
    {
        TimerSystem.OnTimerDepleted += HandleGameOver;
    }

    void OnDisable()
    {
        TimerSystem.OnTimerDepleted -= HandleGameOver;
    }

    void Start()
    {
        if (lose != null) lose.gameObject.SetActive(false);
        if (win != null) win.gameObject.SetActive(false);

        // Purge any empty / null slots assigned by mistake in the Inspector
        targets.RemoveAll(item => item == null);

        if (targets.Count > 0)
        {
            isTracking = true;
        }
    }

    void Update()
    {
        if (!isTracking || hasTriggered || isGameOver || isWon) return;

        // Unity's overridden == null check returns true once a GameObject is destroyed
        targets.RemoveAll(item => item == null);

        if (targets.Count == 0)
        {
            hasTriggered = true;
            isTracking = false;
            onAllDestroyed?.Invoke();
            OnAllDestroyed?.Invoke();
            HandleWin();
        }
    }

    /// <summary>
    /// Adds a target to track at runtime via script.
    /// </summary>
    public void AddTarget(GameObject target)
    {
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            isTracking = true;
            hasTriggered = false;
            isWon = false;
        }
    }

    /// <summary>
    /// Sets a new list of targets to track at runtime via script.
    /// </summary>
    public void SetTargets(List<GameObject> newTargets)
    {
        targets.Clear();
        if (newTargets != null)
        {
            targets.AddRange(newTargets);
            targets.RemoveAll(item => item == null);
        }

        hasTriggered = false;
        isWon = false;
        isTracking = targets.Count > 0;

        if (!isTracking)
        {
            hasTriggered = true;
            onAllDestroyed?.Invoke();
            OnAllDestroyed?.Invoke();
            HandleWin();
        }
    }

    public void HandleGameOver()
    {
        if (isGameOver || isWon) return;
        isGameOver = true;
        hasTriggered = true;
        isTracking = false;

        if (lose != null) lose.gameObject.SetActive(true);

        if (restartDelay > 0f)
        {
            Invoke(nameof(ReloadScene), restartDelay);
        }
        else
        {
            ReloadScene();
        }
    }

    public void HandleWin()
    {
        if (isGameOver || isWon) return;
        isWon = true;
        hasTriggered = true;
        isTracking = false;

        if (win != null)
        {
            win.gameObject.SetActive(true);
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
