using UnityEngine;

public interface IEdgeResponse 
{
    void HandleChaseEdge(Transform target, float chaseSpeed);
    void HandleEdge();
}
