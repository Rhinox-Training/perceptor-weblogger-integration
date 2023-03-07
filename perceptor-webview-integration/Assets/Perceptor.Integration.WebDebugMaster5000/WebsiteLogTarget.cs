using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;
using Rhinox.Utilities;
using System.Threading.Tasks;
using System;
using System.Net.NetworkInformation;

namespace Rhinox.Perceptor
{
    public class WebsiteLogTarget : BaseLogTarget
    {
        private int _sessionID = 1;
        private int _projectID = 1;
        private int interval = 1;
        private float nextTime = 0;
        private bool sessionStarted = false;
        static SessionData _session;
        private bool firstInfoPush = false;

        static Project _project;

        static string _apiData = "";
        static Project projectList;
        
        
        private List<int> _fps= new List<int>();


        // Start is called before the first frame update

        public override async void Update() {
          
           if (sessionStarted) { 
      
            nextTime += Time.deltaTime;

            if (nextTime > 20)
            {
               
                    var info = new UnityInfo();
                    info.sessionId = _sessionID;

                    info.AllocatedMemory = (Profiler.GetTotalAllocatedMemoryLong() / 1048576f).ToString();
                    info.totalReservedMemory = (Profiler.GetTotalReservedMemoryLong() / 1048576f).ToString();
                    info.MonoMemory = (Profiler.GetMonoUsedSizeLong() / 1048576f).ToString();

                    _fps.Add((int)(1f / Time.unscaledDeltaTime));
                    info.fps = _fps[_fps.Count - 1];
             
                    int averagefps = 0;

                    for (int i = 0; i < _fps.Count; i++)
                    {
                        averagefps += _fps[i];
                    }
                    averagefps = averagefps / _fps.Count;
                   
                    info.averageFps = averagefps;
                    _fps.Sort();
                    info.maxFps = _fps[_fps.Count - 1];
                    info.minFps = _fps[0];
                    info.time = (1000 * Time.unscaledDeltaTime).ToString();
                    info.timestamp = System.DateTime.Now.ToString();
                    info.batches = UnityEditor.UnityStats.batches;

                    CreateSessionInfo(info);
                    if (!firstInfoPush)
                    {
                        Application.OpenURL(WebDebuggerConfig.Instance.WebURL +"?sessionId=" + _sessionID);
                        firstInfoPush = true;
                    }
                    nextTime = 0;

                }

        
       }
       
        }


        public WebsiteLogTarget()
        {
            SetupSession();
            
        }

        protected override void OnLog(LogLevels level, string message, Object associatedObject = null)
        {
          
            string shortLevel = ToShortString(level);

            var data = new MessageData();

            data.logType = shortLevel;
            data.message = message;
            data.timestamp = System.DateTime.Now.ToString();
            data.sessionId = _sessionID;
            data.projectId = _projectID;
            data.logger = this.ActiveLogger.GetType().Name.ToString();

            CreateErrorMessage(data);
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

     

        async void SetupSession()
        {

            _project = new Project();
            string[] temp = Application.dataPath.Split('/');
            _project.name = temp[temp.Length - 2];

            var coroutineProjects = new ManagedCoroutine(getProj(WebDebuggerConfig.Instance.ApiURL));

            coroutineProjects.OnFinished += delegate
            {
                Debug.Log(_apiData);
                var projects = JsonHelper.FromJson<Project>("{\"Items\":" + _apiData + "}");

                bool projectExist = false;

                for (int i = 0; i < projects.Length; i++)
                {
                    if (projects[i].name == _project.name)
                    {
                        _project = projects[i];
                        _projectID = projects[i].id;
                        _session.projectId = _project.id;

                        projectExist = true;
                    }
                }

                if (!projectExist)
                {
                    if (projects.Length == 0)
                    {
                        _project.id = 1;
                    }
                    else
                    {
                        _project.id = projects.Length;

                    }
                    _projectID = _project.id;
                    CreateProj(_project);
                }
                _session.projectId = _project.id;

                _session.timestamp = System.DateTime.Now.ToString();
                

                var coroutineSessions = new ManagedCoroutine(getSessions(WebDebuggerConfig.Instance.ApiURL));
                coroutineSessions.OnFinished += delegate
                {
                    var sessions = JsonHelper.FromJson<SessionData>("{\"Items\":" + _apiData + "}");
                    _session.id = sessions.Length + 1;
                    _sessionID = _session.id;

                    CreateSession(_session);



                };
            };




        }


        public void CreateProj(Project proj)
        {
            WWWForm form = new WWWForm();
            form.AddField("name", proj.name);

            WWW www = new WWW(WebDebuggerConfig.Instance.ApiURL + "insertprojects.php", form);
        }

        IEnumerator getProj(string url) {
            WWW itemsData = new WWW(url + "projectsdata.php");
            yield return itemsData;
            _apiData = itemsData.text;
            

        }
        IEnumerator getSessions(string url)
        {

            WWW itemsData = new WWW(url + "sessionsdata.php");
            yield return itemsData;
            _apiData = itemsData.text;


        }
        public void CreateSession(SessionData session)
        {
            WWWForm form = new WWWForm();
            form.AddField("timestamp", session.timestamp);
            form.AddField("projectId", session.projectId);

            WWW www = new WWW(WebDebuggerConfig.Instance.ApiURL + "insertsessions.php", form);
            sessionStarted = true;

        }
        public void CreateSessionInfo(UnityInfo info)
        {
            WWWForm form = new WWWForm();
            form.AddField("timestamp", info.timestamp);
            form.AddField("totalReservedMemory", info.totalReservedMemory);
            form.AddField("AllocatedMemory", info.AllocatedMemory);
            form.AddField("MonoMemory", info.MonoMemory);
            form.AddField("fps", info.fps);
            form.AddField("averageFps", info.averageFps);
            form.AddField("maxFps", info.maxFps);
            form.AddField("minFps", info.minFps);
            form.AddField("time", info.time);
            form.AddField("batches", info.batches);
            form.AddField("sessionId", info.sessionId);

            WWW www = new WWW(WebDebuggerConfig.Instance.ApiURL+"insertsessioninfos.php", form);

           
        }
        public void CreateErrorMessage(MessageData error)
        {
            WWWForm form = new WWWForm();
            form.AddField("logType", error.logType);
            form.AddField("message", error.message);
            form.AddField("timestamp", error.timestamp);
            form.AddField("sessionId", error.sessionId);
            form.AddField("projectId", error.projectId);
            form.AddField("logger", error.logger);

            WWW www = new WWW( WebDebuggerConfig.Instance.ApiURL +"inserterrors.php", form);
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
    public string logger;
    public int id;
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

[System.Serializable]
public class UnityInfo {
    public string timestamp;
    public string totalReservedMemory;
    public string AllocatedMemory;
    public string MonoMemory;

    public int fps;
    public int averageFps;
    public int minFps;
    public int maxFps;
    public string time;

    public int batches;

    public int sessionId;
    public int id;
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