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
    public GameObject Menu;
    public GameObject VRCamera;

    public TMP_InputField Username;
    public TMP_InputField Password;
    public TMP_Text Warning;

    public Button Register;
    public Button Login;

    private bool debounce = true;
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
                // insert data, store token, and start VR world
                string xmlString = response.Substring(response.IndexOf(", data:") + ", data:".Length);

                XmlSerializer serilize_object = new XmlSerializer(typeof(DBPlayer));
                StringReader open_string = new StringReader(xmlString);
                DBPlayer loadInfo = (DBPlayer)serilize_object.Deserialize(open_string);

                Debug.Log(loadInfo.clothing.Length);
                Debug.Log(loadInfo.gold);

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
