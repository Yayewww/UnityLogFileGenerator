using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class UnityLogFileGenerator : MonoBehaviour
{
    public bool bAutoWriteToFile = false; //每當有Log時寫入，注意效能。
    public bool bWriteToFileWhenExit = false; //結束應用程序時產生。
    public bool bRecordAllStack = false;
    public bool bRecordStackByError = false;
    public bool bRecordStackByException = false;
    public bool bRecordStackByWarning = false;
    public bool bRecordStackByAssert = false;
    public bool bRecordStackByLog = false;

    string  totalLog    = "*[FILE]begin log from";
    string  guiLog      = "*[GUI]begin log from";
    string  dirName     = "LogOutput";
    string  logFilePath = "";
    bool    bDoShow      = true;
    int     kChars      = 700;
    int     writeTimes = 0;
    DateTime dtLaunch;
    

    private void Awake()
    {
        dtLaunch = DateTime.Now;
        totalLog = totalLog + "[" + dtLaunch.ToString("yyyy-MM-dd") + "]";
        guiLog   = guiLog + "[" + dtLaunch.ToString("yyyy-MM-dd") + "]";
        InitLogFilePath();
    }
    private void OnEnable() { Application.logMessageReceived  += Log; }
    private void OnDisable() { Application.logMessageReceived -= Log; }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space)) //顯示GUI
        {
            bDoShow = !bDoShow;
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.L)) //手動產生
        {
            WriteLogToFile(totalLog, logFilePath);
        }

    }
    private void OnGUI()
    {
        if (!bDoShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10.0f, 10.0f, 540.0f, 370.0f), guiLog);
    }

    private void OnApplicationQuit()
    {
        if (bWriteToFileWhenExit)
        {
            WriteLogToFile(totalLog, logFilePath);
        }
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        //log時間戳
        var timeStamp = "[" + DateTime.Now.ToLongTimeString() + "]";
        var logContent = timeStamp + logString;

        //根據LogType是否要紀錄stack
        if (bRecordAllStack)
        {
            logContent = logContent + "\n" + stackTrace;
        }
        else
        {
            switch (type)
            {
                case LogType.Error:
                    if (bRecordStackByError)
                    {
                        logContent = logContent + "\n" + stackTrace;
                    }
                    break;
                case LogType.Assert:
                    if (bRecordStackByAssert)
                    {
                        logContent = logContent + "\n" + stackTrace;
                    }
                    break;
                case LogType.Warning:
                    if (bRecordStackByWarning)
                    {
                        logContent = logContent + "\n" + stackTrace;
                    }
                    break;
                case LogType.Log:
                    if (bRecordStackByLog)
                    {
                        logContent = logContent + "\n" + stackTrace;
                    }
                    break;
                case LogType.Exception:
                    if (bRecordStackByException)
                    {
                        logContent = logContent + "\n" + stackTrace;
                    }
                    break;
                default:
                    break;
            }
        }
        totalLog = totalLog + "\n" + logContent;
        guiLog = guiLog + "\n" + timeStamp + logString; //gui不紀錄stackTrace

        //動態寫入文件
        if (bAutoWriteToFile)
        {
            WriteLogToFile(totalLog, logFilePath);
        }

        //自動刪減
        if (guiLog.Length > kChars)
        {
            guiLog = guiLog.Substring(guiLog.Length - kChars);
        }
    }

    private void InitLogFilePath()
    {
        //檢測資料夾
        var logDir = Path.Combine(Application.streamingAssetsPath, dirName);
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        //檢測寫入次數的txt，方便生成Log Index 及 紀錄已經Log幾次了
        var timesPath = Path.Combine(logDir, "LogTimes" + dtLaunch.ToString("yyyy-MM-dd") + ".txt");

        if (!File.Exists(timesPath))
        {
            StreamWriter sw = new StreamWriter(timesPath);
            writeTimes = 0;
            sw.Write(writeTimes.ToString());
            sw.Close();
        }
        else
        {
            StreamReader sr = new StreamReader(timesPath);
            writeTimes = int.Parse(sr.ReadLine());
            writeTimes += 1;
            sr.Close();
        }

        FileStream fs = new FileStream(timesPath, FileMode.Create, FileAccess.ReadWrite);
        StreamWriter tsw = new StreamWriter(fs);
        tsw.Write(writeTimes.ToString());
        tsw.Close();

        //定義Log文件的檔名、組合路徑
        var fileName = "Log_"+ dtLaunch.ToString("yyyy-MM-dd") + "_" + writeTimes.ToString() + ".txt";
        var filePath = Path.Combine(logDir, fileName);
        logFilePath = filePath;
    }

    private void WriteLogToFile(string logHistory, string path)
    {
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(logHistory);
        sw.Close();
    }
}
