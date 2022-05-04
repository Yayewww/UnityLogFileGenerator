using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LogRecorder : MonoBehaviour
{
    [Header("File Write Every Log")]
    public bool autoWrite = false;
    [Header("File Write When Exit Application")]
    public bool autoWriteWhenExit = false;
    [Header("Stacktrace Option")]
    public bool recordError = false;
    public bool recordException = false;
    public bool recordWarning = false;
    public bool recordAssert = false;
    public bool recordLog = false;
    [Header("Dev Build Only")]
    public bool devBuilOnly = false;

    [Header("UI Object")]
    public Text logText;
    public RectTransform scrollView;
    [Header("UI Char Limits")]
    public int kChars = 99999;

    string _totalLog = "";
    string _outputFolder = "LogOutput";
    string _logFilePath = "";
    DateTime dtLaunch;

#region Unity
    void Awake()
    {
        if (devBuilOnly && Debug.isDebugBuild == false)
        {
            Destroy(this);
            return;
        }
        InitRecorder();
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            var isActive = scrollView.gameObject.activeInHierarchy;
            if (isActive)
            {
                scrollView.gameObject.SetActive(false);
            }
            else
            {
                scrollView.gameObject.SetActive(true);
            }
        }
    }
    void OnApplicationPause(bool pause)
    {
        Debug.Log($"##On Application Pause : {pause}##");
        if (autoWriteWhenExit) WriteLogToFile(_totalLog, _logFilePath);
    }
    void OnApplicationFocus(bool focus)
    {
        //If you want to show this, just uncomment.
        //Debug.Log($"##On Application Focus : {focus}##");
        //if (autoWriteWhenExit) WriteLogToFile(_totalLog, _logFilePath);
    }
    void OnApplicationQuit()
    {
        Debug.Log("##On Application Quit##");
        if (autoWriteWhenExit) WriteLogToFile(_totalLog, _logFilePath);
    }
    #endregion

    void InitRecorder()
    {
        DontDestroyOnLoad(gameObject);
        dtLaunch = DateTime.Now;
        var dateString = dtLaunch.ToString("yyyy-MM-dd HH.mm.ss");

        // Init LogFile
        var logDir = "";
        var platform = Application.platform;
        if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer)
        {
            logDir = Path.Combine(Application.streamingAssetsPath, _outputFolder);
        }
        else
        {
            logDir = Path.Combine(Application.persistentDataPath, _outputFolder);
        }
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        _logFilePath = Path.Combine(logDir, $"Log_{dateString}.txt");

        Application.logMessageReceived += Log;
        //Application.logMessageReceivedThreaded += Log;
        Debug.LogWarning($"*[INIT_MESSEAGE] begin log from [{dateString}]");
        Func<string, string> getOS = osDir =>
        {
#if UNITY_STANDALONE
            osDir = "PC";
#elif UNITY_ANDROID
            osDir = "Android";            
#elif UNITY_IPHONE
            osDir = "iOS";        
#else
            osDir = "Unknow";        
#endif
            return osDir;
        };

        var width = Screen.width;
        var height = Screen.height;
        Debug.LogWarning($"Device Platfform: {getOS("")}");
        Debug.LogWarning($"Device Resolution: {width}x{height}");
        // If there's no EventSystem, can't move UI.
        var obj = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (obj == null)
        {
            Debug.LogError("Missing EventSystem in Scene");
        }
    }
    void Log(string logString, string stackTrace, LogType type)
    {
        var textColor = "<color=white>";
        var recordStack = false;
        switch (type)
        {
            case LogType.Error:
                textColor = "<color=red>";
                recordStack = recordError;
                break;
            case LogType.Assert:
                textColor = "<color=red>";
                recordStack = recordAssert;
                break;
            case LogType.Warning:
                textColor = "<color=yellow>";
                recordStack = recordWarning;
                break;
            case LogType.Log:
                textColor = "<color=white>";
                recordStack = recordLog;
                break;
            case LogType.Exception:
                textColor = "<color=red>";
                recordStack = recordException;
                break;
            default:
                break;
        }
        var timeStamp = $"[{DateTime.Now.ToLongTimeString()}]";
        var logContent = $"\n{textColor}{timeStamp} {logString}</color>";

        if (recordStack)
        {
            logContent += $"\n{stackTrace}";
        }
        _totalLog += logContent;

        var guiLog = _totalLog;
        if (guiLog.Length > kChars)
        {
            guiLog = guiLog.Substring(guiLog.Length - kChars);
        }
        logText.text = guiLog;

        if (autoWrite)
        {
            WriteLogToFile(_totalLog, _logFilePath);
        }
    }
    void WriteLogToFile(string totalLog, string path)
    {
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(totalLog);
        sw.Close();
    }

}
