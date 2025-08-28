using UnityEngine;

public class Broom : Interactable
{
    public override void Interact()
    {
        GameManager._instance.hasBroom = true;
    }
}
