using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform tarjetToAttack;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && tarjetToAttack == null)
        {
            tarjetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && tarjetToAttack != null)
        {
            tarjetToAttack = null;
        }
    }
}
