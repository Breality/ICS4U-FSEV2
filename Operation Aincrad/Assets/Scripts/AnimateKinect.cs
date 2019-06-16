using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class AnimateKinect : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform upperBodyRoot;
    private const int NUM_UPPERBODYROOT = 18;
    private List<string> EquipmentTypes = new List<string>() { "Swords Right", "Swords Left", "Armor", "Boots Right", "Boots Left", "Pendants","Helmets"};
    public void Update()
    {
        if (isLocalPlayer)
        {
            var posState = new Vector3[NUM_UPPERBODYROOT];
            var rotState = new Quaternion[NUM_UPPERBODYROOT];

            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(upperBodyRoot);
            int curIndex = 0;


            while (curIndex < NUM_UPPERBODYROOT)
            {
                Transform curT = que.Dequeue();
                if (!EquipmentTypes.Contains(curT.name))
                {
                    posState[curIndex] = curT.localPosition;
                    rotState[curIndex] = curT.localRotation;
                    curIndex += 1;


                    foreach (Transform child in curT)
                    {
                        que.Enqueue(child);
                    }

                }
            }
            // collect state here
            CmdSendState(posState, rotState);
        }
    }
    
    [Command]
    public void CmdSendState(Vector3[] childPositions, Quaternion[] childRotations)
    {
        RpcReceiveState(childPositions, childRotations);
    }

    [ClientRpc]
    public void RpcReceiveState(Vector3[] childPositions, Quaternion[] childRotations)
    {
        if (!isLocalPlayer)
        {
            int curIndex = 0;
            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(upperBodyRoot);
   


            while (curIndex < NUM_UPPERBODYROOT)
            {
                Transform curT = que.Dequeue();
                if (!EquipmentTypes.Contains(curT.name))
                {
                    curT.localPosition = childPositions[curIndex];
                    curT.localRotation = childRotations[curIndex];
                    curIndex += 1;


                    foreach (Transform child in curT)
                    {
                        que.Enqueue(child);
                    }
                }


            }
        }
    }
}
