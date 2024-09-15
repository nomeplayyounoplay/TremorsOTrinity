using KBCore.Refs;
using UnityEngine;

namespace RetroHorror
{
    public class PlayerObjectPickup: ValidatedMonoBehaviour
    {
        [Header("References")]
        [SerializeField, Self] Transform playerHand;
        GameObject currentObject;
        bool isHoldingObject = false;

        public void AttemptPickup()
        {
            // if(isHoldingObject) DropObject();

            // else
            // {
            //     PickupObject()
            // }

            //This is where we can check weight/size
        }

        public void PickupObject(GameObject objectToCarry)
        {
        //     Debug.Log("Pickup");

        //     if(isHoldingObject) DropObject();
        //    // if(!objectToCarry) return;
        //     currentObject = objectToCarry;

        //     currentObject.transform.SetParent(playerHand);

        //     //OPTIONAL:
        //     //These are for tweaking location and position RELATIVE to the player's handbone we attached the item to
        //     //The items themselves may need to have these values so we can specifically set how they are to be held
        //     //There may be a better way to do all of this either way, we shall see
        //     currentObject.transform.localPosition = Vector3.zero;  //Sets to center of handbone (0,0,0)
        //     currentObject.transform.localRotation = Quaternion.identity;

        //     //Stop physics from making it go crazy
        //     currentObject.GetComponent<Rigidbody>().isKinematic = true;

        //     isHoldingObject = true;

        }

        void DropObject()
        {
            // Debug.Log("Drop");

            // if(!currentObject) return;

            // currentObject.transform.SetParent(null);

            // currentObject.GetComponent<Rigidbody>().isKinematic = false;

            // currentObject = null;
            // isHoldingObject = false;
        }
    }
}

