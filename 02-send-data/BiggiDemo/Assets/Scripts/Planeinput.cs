using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
//Needs the package com.unity.nuget.newtonsoft-json. Best C# json serialisation library
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class Planeinput : MonoBehaviour
{
    private int frameCounter = 0;
    private int score = 0;
    private List<Tuple<int, string>> actionLog = new List<Tuple<int, string>>();
    private Rigidbody2D m_Rigidbody2D;
    InputAction tapAction;
    InputAction resetAction;
    private int tapsMade = 0;

    private bool inputEnabled = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        tapAction = InputSystem.actions.FindAction("Tap");
        resetAction = InputSystem.actions.FindAction("Reset");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        score++;
        actionLog.Add(new Tuple<int, string>(frameCounter, "PassedGap"));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        col.collider.enabled = false;
        m_Rigidbody2D.angularVelocity = 180;
        m_Rigidbody2D.linearVelocityY = 4;
        m_Rigidbody2D.linearVelocityX = -2;
        GetComponent<Collider2D>().enabled = false;
        inputEnabled = false;
        actionLog.Add(new Tuple<int, string>(frameCounter, "Died"));
        StartCoroutine(Upload());
    }

    private GUIStyle uploadStyle;

    private void OnGUI()
    {
        uploadStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        if (!isUploading && !inputEnabled)
        {
            GUI.Label(
                new Rect(0, 0, Screen.width, Screen.height),
                "Press R to restart",
                uploadStyle
            );
            return;
        }

        if (!isUploading)
            return;


        GUI.Label(
            new Rect(0, 0, Screen.width, Screen.height),
            "UPLOADING DATA...",
            uploadStyle
        );
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isUploading && !inputEnabled && resetAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        frameCounter++;
        if (!inputEnabled)
            return;
        if (tapAction.WasPressedThisFrame())
        {
            m_Rigidbody2D.linearVelocityY = 4;
            actionLog.Add(new Tuple<int, string>(frameCounter, "Tapped"));
            tapsMade++;
        }
    }

    [Serializable]
    public class APIResponse
    {
        public string url;
        public Dictionary<string, string> data;
    }


    public bool isUploading;

    [Serializable]
    public class Results
    {
        public int gapsPassed;
        public float tapsMade;
    }

    [Serializable]
    public class DetailedResults
    {
        public int gapsPassed;
        public float tapsMade;
        public List<string> actionLog;
    }

    IEnumerator Upload()
    {
        //Is Uploading could be used for UI.
        isUploading = true;


        //Build a dictionary of our data
        Results results = new Results();
        results.gapsPassed = score;
        results.tapsMade = tapsMade;


        var jsonBody = JsonUtility.ToJson(results);
        Debug.Log(jsonBody);
        //Setting up the request properly here to POST to our API endpoint!
        UnityWebRequest www =
            new UnityWebRequest("https://cigsfxmew6.execute-api.eu-west-2.amazonaws.com/api/store", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            isUploading = false;
            yield break;
        }

        Debug.Log(www.downloadHandler.text);
        //We get some JSON back
        var data = JsonConvert.DeserializeObject<APIResponse>(
            www.downloadHandler.text
        );
        Debug.Log(data);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        foreach (var kvp in data.data)
        {
            formData.Add(new MultipartFormDataSection(kvp.Key, kvp.Value));
        }

        DetailedResults detailedResults = new DetailedResults();
        detailedResults.gapsPassed = score;
        detailedResults.tapsMade = tapsMade;
        detailedResults.actionLog = actionLog.Select(x => x.Item1 + "," + x.Item2).ToList();
        //Add the bigger data to the dictionary
        var fullReplayData = JsonUtility.ToJson(detailedResults);

        //Needs bytes to be sent up
        var dataAsBytes = Encoding.Unicode.GetBytes(fullReplayData);
        formData.Add(
            new MultipartFormFileSection("file", dataAsBytes, data.data["key"], "application/octet-stream"));

        www = UnityWebRequest.Post(data.url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }

        isUploading = false;
    }
}