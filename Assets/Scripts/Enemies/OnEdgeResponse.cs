using UnityEngine;

public class OnEdgeResponse : MonoBehaviour, IEdgeResponse
{
    [SerializeField] private EnemyMovement enemyMovement;
    public void OnEdgeDetected() => enemyMovement.Flip(); 
}
