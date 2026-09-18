using UnityEngine;
using System.Collections.Generic;

public class Breakable : MonoBehaviour
{
    [SerializeField] private List<GameObject> breakableParts;
    [SerializeField] private float breakForce = 5f;
    [SerializeField] private float breakTorque = 10f;

    public void Break()
    {
        GetComponent<Collider2D>().enabled = false;
        foreach (GameObject part in breakableParts)
        {
            part.SetActive(true);
            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody2D not found on breakable part.");
                continue;
            }
            rb.constraints = RigidbodyConstraints2D.None; // Remove constraints to allow free movement
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * breakForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-breakTorque, breakTorque), ForceMode2D.Impulse);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Breakable hit by: " + collision.name);
        if (collision.CompareTag("Player"))
        {
            Break();
        }
    }
}