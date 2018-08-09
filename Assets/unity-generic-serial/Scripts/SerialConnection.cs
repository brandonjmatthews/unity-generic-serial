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


namespace UGS
{
    [Serializable]
    public enum WaitFor
    {
        EndOfFrame,
        DataAvailable,
    }


    public class SerialConnection : MonoBehaviour
    {

        public string _portName = "COM1";
        public int _baudRate = 9600;
        public int _dataBits = 8;
        public StopBits _stopBits = StopBits.One;
        public Parity _parity = Parity.None;
        public Handshake _handshape = Handshake.RequestToSend;

        public WaitFor waitFor = WaitFor.EndOfFrame;
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
                if (_serialPort != null)
                {
                    _serialPort.PortName = _portName;
                }

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
                if (_serialPort != null)
                {
                    _serialPort.BaudRate = _baudRate;
                }

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
                if (_serialPort != null)
                {
                    _serialPort.DataBits = _dataBits;
                }

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
                if (_serialPort != null)
                {
                    _serialPort.StopBits = _stopBits;
                }

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
                if (_serialPort != null)
                {
                    _serialPort.Parity = _parity;
                }
                if (autoCycle) Cycle(_maxOpenAttempts);
            }
        }

        void Start()
        {
            if (autoCycle)
            {
                Debug.Log("UGS: If Auto Cycle is enabled, the port will close and attempt to re-open whenever a propery is changed through a script. If a property is changed in inspector, the port must be cycled manually.");
            }
        }

        public void Flush()
        {
            if (_serialPort != null)
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
            }
        }

        public bool Open(int attempts = 0)
        {
            if (attempts < _maxOpenAttempts)
            {
                if (_serialPort != null)
                {
                    _serialPort = null;
                    return Open(attempts);
                }
                else
                {
                    try
                    {
                        CreateSerialPort();
                        _serialPort.Open();
                        _serialPort.ReadTimeout = _readTimeout;
                        _serialPort.WriteTimeout = _writeTimeout;
                        _started = true;
                        onSerialOpen.Invoke();
                        readRoutine = StartCoroutine(Read());
                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("UGS: Failed to Open Port, Attempts remaining: {0}", _maxOpenAttempts - (attempts + 1));
                        Debug.LogError(e.Message);
                        return Open(attempts + 1);
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("UGS: Port failed to open after {0} attempts", _maxOpenAttempts);
                return false;
            }
        }

        void CreateSerialPort()
        {
            string formattedPort = FormatPortName(_portName);
            Debug.Log("UGS: Opening port: " + formattedPort);
            if (Array.IndexOf(SerialPort.GetPortNames(), _portName) == -1)
            {
                throw new Exception("UGS: Serial port " + _portName + " (" + formattedPort + ") is not available.");
            }
            Debug.Log(_baudRate);
            _serialPort = new SerialPort(formattedPort, _baudRate, _parity, _dataBits, _stopBits);
        }

        static string FormatPortName(string name)
        {
            string formattedPort = name;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            formattedPort = "\\\\\\\\.\\\\" + name;
#endif
            return formattedPort;
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


                if (waitFor == WaitFor.EndOfFrame)
                {
                    yield return new WaitForEndOfFrame();
                }
                else if (waitFor == WaitFor.DataAvailable)
                {
                    yield return new WaitUntil(() => _serialPort.BytesToRead > 0);
                }
                else
                {
                    // Default
                    yield return new WaitForEndOfFrame();
                }

            }

        }

        public void Write(byte[] data)
        {
            Debug.Log("writing: " + (char)data[0]);
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

        public void WriteLine(string s)
        {

            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    Debug.Log("Sending: " + s);
                    _serialPort.WriteLine(s);
                }
                catch (TimeoutException)
                {
                    Debug.LogError("UGS: Write timed out");
                }
            }

        }

        void OnApplicationQuit()
        {
            Close();
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
                _serialPort = null;
                Open(attempts);
            }
        }
    }

}