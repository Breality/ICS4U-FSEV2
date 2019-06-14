/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Menu Toggle Class
 * Description:
 * This class handles the HTTP requests sent by the client. The server listens to these requests for anything regarding account data. 
 */

// Importing all the required modules/packages/scripts 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System;
using Mirror;

public class HTTPClient : MonoBehaviour
{
    // ------------ Variables ------------
    public InfoCenter infoCenter; 
    public GameObject Menu; // The login menu
    public GameObject VRCamera; // The camera that needs to activate for VR

    // input fields for the user to insert information
    public TMP_InputField Username;
    public TMP_InputField Password;
    public TMP_Text Warning; // warning text that is changed to display any problems with the selection 

    // button references
    public Button Register;
    public Button Login;

    private bool debounce = true; // used for ensuring they don't click a button while the other resolves 
    private string token; // every player has a token that is stored in their HTTPClient

    private DBPlayer loadedData; // The loaded DBPlayer class, the class used for sending player info with requests
    private Dictionary<string, Dictionary<string, string>> loadedEquip; // The equipment info from the server's text file 
    [SerializeField]
    private Mirror.NetworkManager netManager; // this is for timing the login with the movement networking 

    // Listen for buttons from the start of the game
    void Start()
    {
        Register.onClick.AddListener(delegate { ButtonClicked("register"); });
        Login.onClick.AddListener(delegate { ButtonClicked("login"); });
    }
    
    // callback function for when a button is clicked
    void ButtonClicked(string request) 
    {
        EventSystem.current.SetSelectedGameObject(null); // remove the highlighted effect unity makes for selection
        if (debounce) // checks if they are spamming
        {
            debounce = false;
            StartCoroutine(Upload(request, Username.text, Password.text)); // start a seperate thread to upload this form as an http request
        }
    }

    // This function handle sign in and register
    IEnumerator Upload(string request, string username, string password)
    {
        // Creates the parameters through fields for the form
        WWWForm form = new WWWForm();
        form.AddField("request", request);
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post("http://209.182.232.50:1234/", form); // sends a post request to the server
        yield return www.SendWebRequest(); // wait for a response

        if (www.isNetworkError || www.isHttpError) // error took place 
        {
            debounce = true;
            Debug.Log(www.error);
        }
        else
        { // response was recieved
            string response = www.downloadHandler.text;
            Debug.Log("We got a response");

            if (response.Contains("success, token")) // login or register was successful 
            {
                try
                {
                    // reading the data sent in the response 
                    // data is sent as a string with this formatting: "Creation success, token:" + randToken + ", Equipment Keys:" + EquipmentXML1 +
                    // ", Equipment Values:" + EquipmentXML2 + ", Player Data:" + xmlString"
                    // The dictionary is split into an array of keys and an array of values for xml format 

                    // finding the important numbers for token
                    int startToken = response.IndexOf("token:") + "token:".Length;
                    int startEquip = response.IndexOf(", Equipment Keys:");
                    token = response.Substring(startToken, startEquip - startToken); // setting the token variables 

                    // Finding the important indexes for the equipment dictionary keys
                    startEquip += ", Equipment Keys:".Length;
                    int EndKeys = response.IndexOf(", Equipment Values:");
                    string equipKeys = response.Substring(startEquip, EndKeys - startEquip); // setting the variable

                    // Finding the important indexes for the equipment dictionary values
                    EndKeys += ", Equipment Values:".Length;
                    int startData = response.IndexOf(", Player Data:");
                    string equipVals = response.Substring(EndKeys, startData - EndKeys); // setting the variable
                    string DataString = response.Substring(startData + ", Player Data:".Length); // set the player data string

                    // debug in case things look odd
                    Debug.Log(equipKeys);
                    Debug.Log(equipVals);
                    Debug.Log(DataString);

                    //  parsing through the xml textx
                    XmlSerializer serilize_object = new XmlSerializer(typeof(string[][]));
                    StringReader open_string = new StringReader(equipKeys);
                    string[][] loadedKeys = (string[][])serilize_object.Deserialize(open_string);

                    open_string = new StringReader(equipVals);
                    string[][] loadedVals = (string[][])serilize_object.Deserialize(open_string);

                    serilize_object = new XmlSerializer(typeof(DBPlayer));
                    open_string = new StringReader(DataString);
                    loadedData = (DBPlayer)serilize_object.Deserialize(open_string);

                    // reformating the dictionary 
                    loadedEquip = new Dictionary<string, Dictionary<string, string>> {
                        {"Weapons", new Dictionary<string, string> { } },
                        {"Items", new Dictionary<string, string> { } },
                        {"Clothing", new Dictionary<string, string> { } },
                    };

                    for (int a=0; a<3; a++)
                    {
                        for (int i = 0; i < loadedKeys[a].Length; i++)
                        {
                            loadedEquip[new string[] { "Clothing", "Weapons", "Items" }[a]][loadedKeys[a][i]] = loadedVals[a][i];
                        }
                    }

                    // calling the functions 
                    Menu.SetActive(false);
                    netManager.StartClient();

                }catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                
            }
            else
            {
                // let them know of the issue
                Warning.text = response;
                Warning.transform.gameObject.SetActive(true);
                debounce = true;
                yield return new WaitForSeconds(3.5f);
                if (Warning.text.Equals(response))
                {
                    Warning.transform.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private IEnumerator SendMessage(Dictionary<string, string> parameters)
    {
        // filling out the form appropriately
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            form.AddField(parameter.Key, parameter.Value);
        }

        // sending the server the form
        UnityWebRequest www = UnityWebRequest.Post("http://209.182.232.50:1234/", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;

            // figure out what to do next or what to ping
            if (parameters["request"].Equals("stats")) // we asked the server for our stats because they told us to in UDP
            {
                try
                {
                    XmlSerializer serilize_object = new XmlSerializer(typeof(DBPlayer));
                    StringReader open_string = new StringReader(response);
                    DBPlayer newStats = (DBPlayer)serilize_object.Deserialize(open_string);
                    infoCenter.NewStats(newStats);
                }
                catch(Exception e) // this will error when the request comes back but the server didn't send us the info, probably because they've already been logged out
                {
                    Debug.Log(e.ToString());
                }
                
            }
        }
    }

    public void AskServer(Dictionary<string, string> parameters)
    {
        StartCoroutine(SendMessage(parameters));
    }

    void OnApplicationQuit()
    {
        AskServer(new Dictionary<string, string> { { "request", "logout"} });
    }
    public DBPlayer GetLoadedD()
    {
        return loadedData;
    }
    public Dictionary<string, Dictionary<string, string>> GetLoadedEquip()
    {
        return loadedEquip;
    }
}
