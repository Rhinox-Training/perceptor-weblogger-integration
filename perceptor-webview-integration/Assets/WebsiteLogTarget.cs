using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;
using Rhinox.Utilities;
using System.Threading.Tasks;
using System;

namespace Rhinox.Perceptor
{
    public class WebsiteLogTarget : BaseLogTarget
    {
        static int _sessionID;

        static SessionData _session;

        static Project _project;

        static string _apiData = "";
        static Project projectList;

        
        // Start is called before the first frame update

        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethod()
        {
            WebsiteLogTarget temp = new WebsiteLogTarget();
            SetupSession();
            Debug.Log("start");
            //Debug.Log("trying to open api");
            Application.OpenURL("file:///C:/api/main.html");
            //StartCoroutine(Upload("http://localhost:3000/errors", "message"));
        }

        protected override void OnLog(LogLevels level, string message, Object associatedObject = null)
        {

            string shortLevel = ToShortString(level);

            var data = new MessageData();

            data.logType = shortLevel;
            data.message = message;
            data.timestamp = System.DateTime.Now.ToString();
            data.sessionId = _sessionID;

            new ManagedCoroutine(PutRequest("http://localhost:3000/errors", TurnObjectToJsonString(data)));

        }

        static string ToShortString(LogLevels level)
        {
            switch (level)
            {
                case LogLevels.Trace:
                    return "TRAC";
                case LogLevels.Debug:
                    return "DEBG";
                case LogLevels.Info:
                    return "INFO";
                case LogLevels.Warn:
                    return "WARN";
                case LogLevels.Error:
                    return "ERRO";
                case LogLevels.Fatal:
                    return "FATL";
                case LogLevels.None:
                    return null;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        static IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                _apiData = webRequest.downloadHandler.text;
                
                string[] pages = uri.Split('/');
                int page = pages.Length - 1;
                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                }


            }
        }

        static IEnumerator PutRequest(string url, string data)
        {

            //Turn message data to Json

            Debug.Log("pushing : " + data);

            //Start webrequest pushing API with Put method
            using (UnityWebRequest www = UnityWebRequest.Put(url, data))
            {
                //Change to POST
                www.method = "POST";
                //Set Headers so api accepts the data
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Accept", "*/*");
                yield return www.SendWebRequest();

                //Check for API error
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                }
            }
        }

        static async void SetupSession()
        {

            _project = new Project();
            string[] temp = Application.dataPath.Split('/');
            _project.name = temp[temp.Length - 2];

            // new ManagedCoroutine(PutRequest("http://Localhost:3000/projects", TurnObjectToJsonString(_project)));


            //Get projects;
            var coroutine = new ManagedCoroutine(GetRequest("http://Localhost:3000/projects"));
            //system will wait here



            coroutine.OnFinished += delegate {
                if (_apiData != "[]")
                {

                    //projectList = JsonUtility.FromJson<Projects>("{\"projects\":" + _apiData + "}"); 
                    var projectList = JsonHelper.FromJson<Project>("{\"Items\":" + _apiData + "}");



                    Debug.Log("project count " + projectList.Length);

                    foreach (var project in projectList)
                    {
                        Debug.Log("searching for project");
                        if (project.name == _project.name)
                        {
                            Debug.Log("project found");
                            _project = project;
                            _session.projectId = project.id;
                            break;
                        }

                        else
                        {
                            Debug.Log("creating new project");
                            _session.projectId = 1;
                            new ManagedCoroutine(PutRequest("http://Localhost:3000/projects", TurnObjectToJsonString(_project)));

                        }
                    }
                }
                else
                {
                    Debug.Log("creating first project ");
                    _project.id = 1;
                    new ManagedCoroutine(PutRequest("http://Localhost:3000/projects", TurnObjectToJsonString(_project)));


                }


                _session.timestamp = System.DateTime.Now.ToString();
                _session.projectId = _project.id;


                new ManagedCoroutine(PutRequest("http://Localhost:3000/sessions", TurnObjectToJsonString(_session)));


                LogLevels level = LogLevels.Error;

                string shortLevel = ToShortString(level);

               

                coroutine = new ManagedCoroutine(GetRequest("http://localhost:3000/sessions"));

                coroutine.OnFinished += delegate {

                    var sessionList = JsonHelper.FromJson<SessionData>("{\"Items\":" + _apiData + "}");


                    _sessionID = sessionList.Length;

                    var data = new MessageData();

                    data.logType = shortLevel;
                    data.message = "this is a test";
                    data.timestamp = System.DateTime.Now.ToString();
                    data.sessionId = _sessionID;
                    data.projectId = _project.id;

                    new ManagedCoroutine(PutRequest("http://localhost:3000/errors", TurnObjectToJsonString(data)));
                };

            };
            //If doesn't exist add it to the api
            //Set up session


          

        }

        static string TurnObjectToJsonString<T>(T objectRef)
        {
            return JsonUtility.ToJson(objectRef);
        }
    }

}

[System.Serializable]
public struct MessageData
{
    public string logType;
    public string message;
    public string timestamp;
    public int sessionId;
    public int projectId;
}

[System.Serializable]
public struct SessionData
{
    public string timestamp;
    public int projectId;
    public int id;
}

[System.Serializable]
public class Project
{
    public string name;
    public int id;
}
[System.Serializable]
public class Projects { 
    public List<Project> Items;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}