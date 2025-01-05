using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MEET_AND_TALK
{
    [CreateAssetMenu(menuName = "Dialogue/Event/Trigger Event")]
    [System.Serializable]
    public class TriggerEvent : DialogueEventSO
    {
       public string GOName; // The name of the GameObject to trigger the event on. Must have a ActionEvents component attached to it. Only one of the gameobjects with this name should ever be active at any time in the game.

       public override void RunEvent()
       {
           DialogueEventManager.Instance.TriggerEvent(GOName);
           base.RunEvent();
       }
    }
}
