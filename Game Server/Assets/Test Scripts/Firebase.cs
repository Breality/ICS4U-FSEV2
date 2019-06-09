/*using Proyecto26;
using RSG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firebase : MonoBehaviour
{
    private void PostToDatabase(DBPlayer user)
    {
        RestClient.Put("https://ics4u-748c2.firebaseio.com/" + user.username +".json", user);
    }

    private IEnumerator printScore(string user)
    {
        RestClient.Get<DBPlayer>("https://ics4u-748c2.firebaseio.com/" + user + ".json").Then(response =>
        {
            if (response == null)
            {
                Debug.Log(user + " does not exist");
            }
            else
            {
                Debug.Log(user + " has " + response.score);
            }
        });
        
        yield return 0;
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(printScore("nooriscool123"));
        //PostToDatabase(new DBPlayer("nooriscool123", 150));
        //StartCoroutine(printScore("hello"));
        //Debug.Log("Started routines");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/