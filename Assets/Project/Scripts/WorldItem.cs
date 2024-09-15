using UnityEngine;

namespace RetroHorror
{   
    [CreateAssetMenu(fileName = "World Item", menuName = "RetroHorror/WorldItems")]
    public class WorldItem : ScriptableObject 
    {
        public Transform prefab;
        public Sprite sprite;
        public string objectName;   
    }

    public class CarriableItem : WorldItem, Interactable
    {   
        public void Interact()
        {

        }
    }

    public class Rock : CarriableItem
    {

    }

    public class NonCarriableItem : WorldItem
    {

    }

    public class Door : NonCarriableItem
    {
        
    }
}