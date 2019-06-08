using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Transform characterPos;

    Vector3 relativeCamPos = Vector3.zero;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.localPosition != relativeCamPos)
        {
            characterPos.position += (this.transform.localPosition-relativeCamPos);
            this.transform.localPosition = relativeCamPos;
        }
    }
    public void setRelativeCamPos(Vector3 pos)
    {
        relativeCamPos = pos;
    }
}
