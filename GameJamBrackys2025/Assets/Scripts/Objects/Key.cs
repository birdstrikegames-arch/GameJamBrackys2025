using UnityEngine;

public class Key : Interactable
{
    public override void Interact()
    {
        GameManager._instance.hasKey = true;
    }


}
