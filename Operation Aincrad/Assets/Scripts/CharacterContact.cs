using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterContact : MonoBehaviour
{
    List<GameObject> currentCollisions = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        currentCollisions.Add(collision.gameObject);
        Debug.Log(collision.gameObject.name);
    }
    private void OnCollisionExit(Collision collision)
    {
        currentCollisions.Remove(collision.gameObject);
    }
    public List<GameObject> GetCurCollisions()
    {
        return currentCollisions;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
