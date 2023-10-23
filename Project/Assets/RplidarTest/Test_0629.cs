using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Ports;
using System.Threading;

public class Test_0629 : MonoBehaviour
{
    public string portName = "COM3"; // RPLIDAR A2의 포트 이름
    public int baudRate = 115200; // 통신 속도
    public SerialPort serialPort; // 시리얼 포트 객체
    private Thread thread; // 스레드 객체

    void Start()
    {
        OpenSerialPort(); // 시리얼 포트 열기
        Debug.Log("START");
    }

    void Update()
    {
        // 프레임마다 RPLIDAR A2 데이터를 읽고 처리
    }

    void OpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            serialPort.ReadTimeout = 10;
            serialPort.WriteTimeout = 10;

            thread = new Thread(ReadSerialData);
            thread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to open serial port: " + ex.Message);
        }
    }

    void ReadSerialData()
    {
        try
        {
            while (serialPort.IsOpen)
            {
                string line = serialPort.ReadLine();
                Debug.Log("Received data: " + line);
            }
        }
        catch (TimeoutException)
        {
            // Timeout 예외 처리
        }
        catch (Exception ex)
        {
            Debug.LogError("Serial port error: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
        }
    }
}
