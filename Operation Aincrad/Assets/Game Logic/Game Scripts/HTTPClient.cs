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
    // Variables
    public InfoCenter infoCenter; 
    public GameObject Menu;
    public GameObject VRCamera;

    public TMP_InputField Username;
    public TMP_InputField Password;
    public TMP_Text Warning;

    public Button Register;
    public Button Login;

    private bool debounce = true;
    private string token;
    [SerializeField]
    private Mirror.NetworkManager netManager;
    // Listen for buttons
    void Start()
    {
        Register.onClick.AddListener(delegate { ButtonClicked("register"); });
        Login.onClick.AddListener(delegate { ButtonClicked("login"); });
    }
    
    // handle button clicks
    void ButtonClicked(string request)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (debounce)
        {
            debounce = false;
            StartCoroutine(Upload(request, Username.text, Password.text));

        }
    }

    // handle sign in and register
    IEnumerator Upload(string request, string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("request", request);
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post("http://209.182.232.50:1234/", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            debounce = true;
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;
            Debug.Log("We got a response");

            if (response.Contains("success, token"))
            {
                try
                {
                    //"Login success, token:" + randToken + ", Equipment Keys:" + EquipmentXML1 + 
                    //", Equipment Values:" + EquipmentXML2 + ", Player Data:" + xmlString);

                    // insert data, store token, and start VR world
                    int startToken = response.IndexOf("token:") + "token:".Length;
                    int startEquip = response.IndexOf(", Equipment Keys:");// + ", Equipment Data:".Length;
                    token = response.Substring(startToken, startEquip - startToken);

                    startEquip += ", Equipment Keys:".Length;
                    int EndKeys = response.IndexOf(", Equipment Values:");
                    string equipKeys = response.Substring(startEquip, EndKeys - startEquip);

                    EndKeys += ", Equipment Values:".Length;
                    int startData = response.IndexOf(", Player Data:");
                    string equipVals = response.Substring(EndKeys, startData - EndKeys);
                    string DataString = response.Substring(startData + ", Player Data:".Length);

                    Debug.Log(equipKeys);
                    Debug.Log(equipVals);
                    Debug.Log(DataString);

                    XmlSerializer serilize_object = new XmlSerializer(typeof(string[][]));
                    StringReader open_string = new StringReader(equipKeys);
                    string[][] loadedKeys = (string[][])serilize_object.Deserialize(open_string);

                    open_string = new StringReader(equipVals);
                    string[][] loadedVals = (string[][])serilize_object.Deserialize(open_string);

                    serilize_object = new XmlSerializer(typeof(DBPlayer));
                    open_string = new StringReader(DataString);
                    DBPlayer loadedData = (DBPlayer)serilize_object.Deserialize(open_string);

                    Dictionary<string, Dictionary<string, string>> loadedEquip = new Dictionary<string, Dictionary<string, string>> {
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
                    Menu.SetActive(false);
                    netManager.StartClient();
                    infoCenter.LogIn(loadedData, loadedEquip);
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
}
