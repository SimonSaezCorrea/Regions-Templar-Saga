using UnityEngine;

public class Unit : MonoBehaviour
{
    private void OnEnable()
    {
        var mgr = UnitSelectionManager.Instance ?? FindObjectOfType<UnitSelectionManager>();
        if (mgr != null)
            mgr.allUnitsList.Add(gameObject);
    }

    private void OnDisable()
    {
        var mgr = UnitSelectionManager.Instance ?? FindObjectOfType<UnitSelectionManager>();
        if (mgr != null)
            mgr.allUnitsList.Remove(gameObject);
    }
}
