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

public class HTTPClient : MonoBehaviour
{
    public InfoCenter infoCenter; // just insert this line of code into any class and it will have client data
    public GameObject Menu;
    public GameObject VRCamera;

    public TMP_InputField Username;
    public TMP_InputField Password;
    public TMP_Text Warning;

    public Button Register;
    public Button Login;

    private bool debounce = true;
    private string token;
    // Start is called before the first frame update
    void Start()
    {
        Register.onClick.AddListener(delegate { ButtonClicked("register"); });
        Login.onClick.AddListener(delegate { ButtonClicked("login"); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonClicked(string request)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (debounce)
        {
            debounce = false;
            StartCoroutine(Upload(request, Username.text, Password.text));

        }
    }

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
            Debug.Log(response);
            if (response.Contains("success, token"))
            {
                Debug.Log(response);
                // insert data, store token, and start VR world
                int startToken = response.IndexOf("token:") + "token:".Length;
                int startEquip = response.IndexOf(", Equipment Data:");// + ", Equipment Data:".Length;
                int startData = response.IndexOf(", Player Data:"); //+ ", Equipment Data:".Length;

                token = response.Substring(startToken, startEquip - startToken);
                startEquip += ", Equipment Data:".Length;
                string EquipString = response.Substring(startEquip, startData-startEquip);
                string DataString = response.Substring(startData + ", Equipment Data:".Length);

                XmlSerializer serilize_object = new XmlSerializer(typeof(Dictionary<string, Dictionary<string, string>>));
                StringReader open_string = new StringReader(EquipString);
                Dictionary<string, Dictionary<string, string>> loadedEquip = (Dictionary<string, Dictionary<string, string>>)serilize_object.Deserialize(open_string);

                serilize_object = new XmlSerializer(typeof(DBPlayer));
                open_string = new StringReader(DataString);
                DBPlayer loadedData = (DBPlayer)serilize_object.Deserialize(open_string);

                infoCenter.LogIn(loadedData, loadedEquip);
                Menu.SetActive(false);
                VRCamera.SetActive(true);
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
}
