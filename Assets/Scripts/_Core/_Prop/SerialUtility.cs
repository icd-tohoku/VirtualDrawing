using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class SerialUtility : SingletonMonoBehaviour<SerialUtility>
{ 
    public struct SerialParameter
    {
        public SerialPort SerialPort;
        public string PortNum;
        public int BaudRate;

        public SerialParameter(SerialPort serialPort, string portNum, int baudRate)
        {
            SerialPort = serialPort;
            PortNum = portNum;
            BaudRate = baudRate;
        }
    }

    private List<SerialParameter> _serialParameters = new List<SerialParameter>();

    //------------------------------------------------------------------------------
    protected override void Init()
    {
        //Task.Run(Read);
    }

    private void OnDisable()
    {
        CloseAll();
    }

    public int Open(string portNum, int baudRate)
    {
        if(_serialParameters.Any(i => i.PortNum == portNum))
        {
            //すでに開いているポートであればそのインデックスを返す
            var serialParameter = _serialParameters.First(i => i.PortNum == portNum);         
            serialParameter.SerialPort.BaudRate = baudRate;
            serialParameter.BaudRate = baudRate;

            return _serialParameters.IndexOf(serialParameter);
        }

        var serialPort = new SerialPort(portNum, baudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 500,
            NewLine = "\n\n",
        };
        serialPort.Open();

        var para = new SerialParameter(serialPort, portNum, baudRate);
        _serialParameters.Add(para);

        return _serialParameters.IndexOf(para);
    }

    private void CloseAll()
    {
        foreach(SerialParameter serialParameter in _serialParameters)
        {
            if(serialParameter.SerialPort != null && serialParameter.SerialPort.IsOpen)
            {
                serialParameter.SerialPort.Close();
                serialParameter.SerialPort.Dispose();
                Debug.Log(serialParameter.PortNum + "のポートを閉じました");
            }
        }
    }

    private void Read()
    {
        while(_serialParameters.Any(i => i.SerialPort.IsOpen))
        {
            foreach(SerialParameter serialParameter in _serialParameters)
            {
                if (!serialParameter.SerialPort.IsOpen) return;

                try
                {
                    var message = serialParameter.SerialPort.ReadLine();
                    Debug.Log(message);
                }
                catch(System.Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }
    }

    public void Write(int index, string message)
    {
        try
        {
            _serialParameters[index].SerialPort.DiscardOutBuffer(); //シリアル通信がフリーズすることがある。

            _serialParameters[index].SerialPort.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void Writeln(int index, string message)
    {
        try
        {
            _serialParameters[index].SerialPort.Write(message + "\n");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void Write(int index, byte[] b)
    {
        _serialParameters[index].SerialPort.Write(b, 0, b.Length);
    }

    
}
