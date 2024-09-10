using UnityEngine;

namespace RetroHorror
{
    public class Object : MonoBehaviour, Interactable
    {
        public void Interact()
        {
            Debug.Log("OBJECT INTERACTION");
        }
    }
}
