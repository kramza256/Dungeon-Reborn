using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Switch : Stuff, IInteractable
{
    public Switch() { 
        Name = "Switch";
    }
    public bool isInteractable { get => isLock; set => isLock = value; }
    [SerializeField]
    bool isOn = false;
    Animator animator;
    public Identity InteracTo;
    IInteractable IInterac { 
        get {
            return InteracTo as IInteractable;
        }
    }
    public void Interact(Player player)
    {
        isOn = !isOn;
        if (isOn)
        {
            Debug.Log("Switch is On");
            IInterac?.Interact(player);
        }
        else
        {
            Debug.Log("Switch is Off");
            IInterac?.Interact(player);
        }
    }
}

