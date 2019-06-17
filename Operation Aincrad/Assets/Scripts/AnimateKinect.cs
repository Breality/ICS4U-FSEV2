using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * AnimateKinect.cs
 * Description: Used to animate kinect joints in synchronization with movement animations across the server. Simply copies the root transformations across.
 */
public class AnimateKinect : NetworkBehaviour
{
    [SerializeField] private Transform upperBodyRoot;//get the root of the body
    private const int NUM_UPPERBODYROOT = 18;//number of joints on player root
    private List<string> EquipmentTypes = new List<string>() { "Swords Right", "Swords Left", "Armor", "Boots Right", "Boots Left", "Pendants","Helmets"};//unnecessary rotation parents in the root
    public void Update()//update the player animations every frame
    {
        if (isLocalPlayer)//if user is client then send root state to server to be replicated
        {
            var posState = new Vector3[NUM_UPPERBODYROOT];//get the positions of joints
            var rotState = new Quaternion[NUM_UPPERBODYROOT];//get the rotations of the joints

            Queue<Transform> que = new Queue<Transform>();//use bfs queue
            que.Enqueue(upperBodyRoot);//start at parent and work down
            int curIndex = 0;//starts at 0


            while (curIndex < NUM_UPPERBODYROOT)//keep looping until list is full
            {
                Transform curT = que.Dequeue();//get the next in queue
                if (!EquipmentTypes.Contains(curT.name))//if it is valid not one of the unwanted holders of equipment
                {
                    //store state of specific joint
                    posState[curIndex] = curT.localPosition;
                    rotState[curIndex] = curT.localRotation;
                    curIndex += 1;//go to next in list


                    foreach (Transform child in curT)//for each child add  it to queue 
                    {
                        que.Enqueue(child);
                    }

                }
            }
            // collect state here
            CmdSendState(posState, rotState);//send state of joints to server to be replicated
        }
    }
    
    [Command]
    public void CmdSendState(Vector3[] childPositions, Quaternion[] childRotations)//Server command that recieves states of joints and sends those to all clients to be replicated
    {
        RpcReceiveState(childPositions, childRotations);
    }

    [ClientRpc]
    public void RpcReceiveState(Vector3[] childPositions, Quaternion[] childRotations)
    {
        if (!isLocalPlayer)//only need to be replicated if the user is not the local player
        {
            //start the queue and bfs except instead of getting we are setting joint states ----------
            int curIndex = 0;
            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(upperBodyRoot);

            while (curIndex < NUM_UPPERBODYROOT)
            {
                Transform curT = que.Dequeue();
                if (!EquipmentTypes.Contains(curT.name))
                {
                    //set joint start
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
