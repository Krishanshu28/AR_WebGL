using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

using Zappar;
using Siccity.GLTFUtility;
public class RemoteContentLoader : MonoBehaviour
{
    [Header("Set These In Inspector")]
    public string jsonURL;                  
    public Transform spawnPoint;              
    public GameObject loadingIndicator;       
    public TMP_Text labelText;         
    public ZapparImageTrackingTarget imageTarget;

    [System.Serializable]
    public class ARContentData
    {
        public string modelUrl;
        public string label;
    }
    private bool contentLoaded = false;
    private void Start()
    {
        StartCoroutine(DownloadJSON());

        imageTarget.OnSeenEvent.AddListener(OnImageFound);
        imageTarget.OnNotSeenEvent.AddListener(OnImageLost);
    }

    ARContentData aRContentData;
    void OnDestroy()
    {
        imageTarget.OnSeenEvent.RemoveListener(OnImageFound);
        imageTarget.OnNotSeenEvent.RemoveListener(OnImageLost);

    }

    void OnImageFound()
    {
        
        if (!contentLoaded )
        {
            Debug.Log("1111111");
            contentLoaded = true;
            StartCoroutine(DownloadJSON());
            //StartCoroutine(DownloadAndLoadModel(aRContentData.modelUrl));
        }
    }

    void OnImageLost()
    {
        
    }

    IEnumerator DownloadJSON()
    {
        UnityWebRequest www = UnityWebRequest.Get(jsonURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching JSON: " + www.error);
            yield break;
        }

        string jsonText = www.downloadHandler.text;
        //Debug.Log("Downloaded JSON: " + jsonText); 

        try
        {
            if(contentLoaded)
            {
                ARContentData data = JsonUtility.FromJson<ARContentData>(jsonText);
                
                //ARContentItem data = JsonUtility.FromJson<ARContentItem>(jsonText);
                if (loadingIndicator) loadingIndicator.SetActive(true);
                StartCoroutine(DownloadAndLoadModel(data.modelUrl));
                if (labelText) labelText.text = data.label;
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON Parse Error: " + ex.Message); 
        }
    }

    IEnumerator DownloadAndLoadModel(string modelUrl)
    {


        if (loadingIndicator) loadingIndicator.SetActive(true);

        UnityWebRequest modelRequest = UnityWebRequest.Get(modelUrl);
        modelRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return modelRequest.SendWebRequest();

        if (modelRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download model: " + modelRequest.error);
            if (loadingIndicator) loadingIndicator.SetActive(false);
            yield break;
        }

        byte[] glbData = modelRequest.downloadHandler.data;
        GameObject model = Importer.LoadFromBytes(glbData);

        if (model != null)
        {
            model.transform.SetParent(spawnPoint);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogError("Model loading failed");
        };

        if (loadingIndicator) loadingIndicator.SetActive(false);
    }
}
