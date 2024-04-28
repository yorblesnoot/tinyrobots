using System.Net.Http;
using TMPro;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System;

public class JiraReporter : MonoBehaviour
{
    [SerializeField] TMP_InputField header;
    [SerializeField] TMP_InputField body;
    [SerializeField] GameObject form;
    [SerializeField] TMP_Text statusAlert;
    [SerializeField] float reportTimeout;
    static DateTime lastReportTime;

    public readonly string apiToken = "ATATT3xFfGF06BV-yJpKNE7Q4znG66HGScgo2-zg7_uYA_WJvaDuCJ-jOzXxrG-zGHJgBhjyeoVD4-1cmREEgp_SalPoQPAXyWa4CsRgz1fJRifqs8mcsNcMjE3sO0W2qIs36toqpKS61vGcHrZX5AdkAHtJaWoxmwnZe_zuS5ehZGTdWm0R9og=63052D57";
    private string authEncode = "ZXRoZXJlb3NAZ21haWwuY29tOkFUQVRUM3hGZkdGMDZCVi15SnBLTkU3UTR6bkc2NkhHU2NnbzItemc3X3VZQV9XSnZhRHVDSi1qT3pYeHJHLXpHSEpnQmhqeWVvVkQ0LTFjbVJFRWdwX1NhbFBvUVBBWHlXYTRDc1JnejFmSlJpZnFzOG1jc05jTWpFM3NPMFcycUlzMzZ0b3FwS1M2MXZHY0hyWlg1QWRrQUh0SmFXb3htd25aZV96dVM1ZWhaR1RkV20wUjlvZz02MzA1MkQ1Nw==";
    private string postURI = "https://yorblesnoot.atlassian.net/rest/api/3/issue";

    private static HttpClient client;

    private void Awake()
    {
        if(client == null)
        {
            client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + authEncode);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }
    public async void SendReport()
    {
        form.SetActive(false);
        if (lastReportTime != null && (System.DateTime.Now - lastReportTime).TotalSeconds < reportTimeout)
        {
            StartCoroutine(ToastStatus(statusAlert, "Something went wrong...", Color.red));
            return;
        }
        var response = await client.PostAsync(postURI, GenerateJSONPayload(header.text, body.text));
        var responseString = await response.Content.ReadAsStringAsync();
        body.text = "";
        header.text = "";
        Debug.Log(responseString);
        if (response.StatusCode == HttpStatusCode.Created)
        {
            lastReportTime = DateTime.Now;
            StartCoroutine(ToastStatus(statusAlert, "Your report has been sent!", Color.green));
        }
        else
        {
            StartCoroutine(ToastStatus(statusAlert, "Something went wrong...", Color.red));
        }
    }

    readonly float fadeDuration = 3f;
    readonly Color32 invisWhite = new(255,255, 255, 0);
    IEnumerator ToastStatus(TMP_Text words, string message, Color color)
    {
        words.text = message;
        words.gameObject.SetActive(true);
        float timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            words.color = Color32.Lerp(color, invisWhite, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        words.gameObject.SetActive(false);
    }

    HttpContent GenerateJSONPayload(string sum, string body)
    {
        Root root = new()
        {
            fields = new()
            {
                project = new()
                {
                    key = "JBLU"
                },
                summary = sum,
                issuetype = new()
                {
                    name = "Task"
                },
                description = new()
                {
                    version = 1,
                    type = "doc",
                    content = new List<Content1>()
                    {
                        new()
                        {
                            type = "paragraph",
                            content = new List<Content2>()
                            {
                                new()
                                {
                                    type = "text",
                                    text = body
                                }
                            }
                        }
                    }
                }
            }
        };
        string json = JsonUtility.ToJson(root);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    [System.Serializable]
    public class Root
    {
        public Fields fields;
    }
    [System.Serializable]
    public class Fields
    {
        public Project project;
        public string summary;
        public IssueType issuetype;
        public Description description;
    }
    [System.Serializable]
    public class Project
    {
        public string key;
    }
    [System.Serializable]
    public class IssueType
    {
        public string name;
    }
    [System.Serializable]
    public class Description
    {
        public int version;
        public string type;
        public List<Content1> content;
    }
    [System.Serializable]
    public class Content1
    {
        public string type;
        public List<Content2> content;
    }
    [System.Serializable]
    public class Content2
    {
        public string type;
        public string text;
    }
}
