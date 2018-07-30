/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018

Main serial controller, data can be read easily through UnityEvents.!--
No parsing is done in the controller, this should be handled by the event listeners.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;


public class SerialConnection : MonoBehaviour
{

    public string _portName = "COM1";
    public int _baudRate = 9600;
    public int _dataBits = 8;
    public StopBits _stopBits = StopBits.One;
    public Parity _parity = Parity.None;
    public Handshake _handshape = Handshake.RequestToSend;

    public bool autoCycle = true;

    public SerialCharEvent onByteRecieved;
    public SerialDataEvent onDataRecieved;
    public SerialDataEvent onLineRecieved;
    public SerialOpenEvent onSerialOpen;
    public SerialCloseEvent onSerialClose;

    int _readTimeout = 200;
    int _writeTimeout = 2000;

    bool _started = false;
    string _availableSerialPorts;
    SerialPort _serialPort;

    int _maxOpenAttempts = 5;

    Coroutine readRoutine;

    List<int> readLineBuffer = new List<int>();

    public bool isOpen
    {
        get
        {
            return _started;
        }
    }

    public string portName
    {
        get
        {
            return _portName;
        }
        set
        {
            _portName = value;
            if (autoCycle) Cycle(_maxOpenAttempts);
        }
    }

    public int baudRate
    {
        get
        {
            return _baudRate;
        }
        set
        {
            _baudRate = value;
            if (autoCycle) Cycle(_maxOpenAttempts);
        }
    }

    public int dataBits
    {
        get
        {
            return _dataBits;
        }
        set
        {
            _dataBits = value;
            if (autoCycle) Cycle(_maxOpenAttempts);
        }
    }

    public StopBits stopBits
    {
        get
        {
            return _stopBits;
        }
        set
        {
            _stopBits = value;
            if (autoCycle) Cycle(_maxOpenAttempts);
        }
    }

    public Parity parity
    {
        get
        {
            return parity;
        }
        set
        {
            _parity = value;
            if (autoCycle) Cycle(_maxOpenAttempts);
        }
    }

    void Start() {
        if (autoCycle) {
            Debug.Log("UGS: If Auto Cycle is enabled, the port will close and attempt to re-open whenever a propery is changed through a script. If a property is changed in inspector, the port must be cycled manually.");
        }
    }

    public void Open(int attempts = 0)
    {
        if (attempts < _maxOpenAttempts)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    if (autoCycle)
                    {
                        Debug.Log("UGS: Port already open, Port will be cycled.");
                        Cycle(attempts);
                    }
                }
                else
                {
                    _serialPort.Open();
                    _serialPort.ReadTimeout = _readTimeout;
                    _serialPort.WriteTimeout = _writeTimeout;
                    _started = true;
                    onSerialOpen.Invoke();
                    readRoutine = StartCoroutine(Read());
                }
            }
            else
            {
                CreateSerialPort();
                Open(attempts + 1);
            }
        }
        else
        {
            Debug.LogErrorFormat("UGS: Port failed to open after {} attempts", _maxOpenAttempts);
        }
    }

    void CreateSerialPort()
    {
        string formattedPort = _portName;

#if UNITY_STANDALONE_WIN
        formattedPort = "\\\\\\\\.\\\\" + _portName;
#endif
        Debug.Log("UGS: Opening port: " + formattedPort);
        if (Array.IndexOf(SerialPort.GetPortNames(), _portName) == -1)
        {
            throw new Exception("UGS: Serial port " + _portName + " (" + formattedPort + ") is not available.");
        }
        _serialPort = new SerialPort(formattedPort, _baudRate, _parity, _dataBits, _stopBits);
    }

    public IEnumerator Read()
    {
        while (_started)
        {
            try
            {
                string data = "";
                if (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();
                    char c = (char)b;
                    onByteRecieved.Invoke(c);

                    data += c;

                    if (b == SerialConstants.CARRIAGE_RETURN || b == SerialConstants.LINE_FEED)
                    {
                        // End of line, parse int array and send
                        if (readLineBuffer.Count > 0)
                        {
                            string line = "";
                            foreach (int i in readLineBuffer)
                            {
                                line += (char)i;
                            }
                            readLineBuffer = new List<int>();
                            onLineRecieved.Invoke(line);
                        }
                    }
                    else
                    {
                        readLineBuffer.Add(b);
                    }
                }

                if (data.Length > 0)
                {
                    onDataRecieved.Invoke(data);
                }
            }
            catch (TimeoutException)
            {
                Debug.LogError("UGS: Read timed out");
            }
            catch (InvalidOperationException)
            {
                Debug.LogError("UGS: The port is not open");
            }

            yield return new WaitForEndOfFrame();
        }

    }

    public void Write(byte[] data)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
            }
            catch (TimeoutException)
            {
                Debug.LogError("UGS: Write timed out");
            }
        }
    }

    public void Close()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            StopCoroutine(readRoutine);
            _serialPort.Close();
            _serialPort = null;
            _started = false;
            onSerialClose.Invoke();
        }
    }

    public void Cycle(int attempts = 0)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            Close();
            Open(attempts);
        }
    }
}
