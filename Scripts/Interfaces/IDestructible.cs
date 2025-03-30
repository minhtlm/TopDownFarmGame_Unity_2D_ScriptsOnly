using UnityEngine;

public interface IDestructible
{
    bool CanInteract(string toolType);
    void Interact(string toolType);
    void OnInteractAnimation();
    Transform GetTransform();
}