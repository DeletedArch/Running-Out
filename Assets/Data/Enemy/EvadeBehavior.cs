using UnityEngine;

[CreateAssetMenu(fileName = "EvadeBehavior", menuName = "Scriptable Objects/EvadeBehavior")]
public class EvadeBehavior : ScriptableObject, CustomeEnemyBehavior
{
    public EnemyContext enemyContext;
    [SerializeField] float coolDownTimer;
    public void Evalute()
    {
        
    }

    // clean the data 
    public void Unload()
    {
        enemyContext = null;
    }

    public void intialisation(EnemyContext enemyContext)
    {
        this.enemyContext = enemyContext;

    }
}
