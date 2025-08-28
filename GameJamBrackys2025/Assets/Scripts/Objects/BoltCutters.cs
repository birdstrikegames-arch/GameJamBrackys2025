using UnityEngine;

public class BoltCutters : Interactable
{
    public override void Interact()
    {
        GameManager._instance.hasBoltCutters = true;
    }
}
