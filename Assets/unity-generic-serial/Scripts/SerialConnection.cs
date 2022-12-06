#define CONNECTIVITY_SERIAL

/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018

Main serial controller, data can be read easily through UnityEvents.!--
No parsing is done in the controller, this should be handled by the event listeners.
 */
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using UnityEngine;

namespace Connectivity
{
    public class SerialConnection : Connection
    {
        public string _portName = "COM1";
        public int _baudRate = 9600;
        public int _dataBits = 8;
        public StopBits _stopBits = StopBits.One;
        public Parity _parity = Parity.None;
        public Handshake _handshake = Handshake.RequestToSend;

        string _availableSerialPorts;
        SerialPort _serialPort;

        Coroutine readRoutine;

    

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

                if (isOpen) Close();
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

                if (isOpen) Close();
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

                if (isOpen) Close();
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

                if (isOpen) Close();
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

                if (isOpen) Close();
            }
        }

        public override void Flush()
        {
            if (_serialPort != null)
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
            }
        }

        public override void Open()
        {
            // Don't try to open a port if this Connection is already open
            if (isOpen) return;

            try
            {
                CreateSerialPort();
                _serialPort.Open();
                _serialPort.ReadTimeout = _readTimeout;
                _serialPort.WriteTimeout = _writeTimeout;
                isOpen = true;
                readRoutine = StartCoroutine(Read());
                onOpen.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("[UGS] Failed to Open Port");
                Debug.LogWarning(e.Message);
                isOpen = false;
            }
        }

        void CreateSerialPort()
        {
            string formattedPort = FormatPortName(_portName);
            Debug.Log("[UGS] Opening port: " + formattedPort);
            if (Array.IndexOf(SerialPort.GetPortNames(), _portName) == -1)
            {
                throw new Exception("[UGS] Serial port " + _portName + " (" + formattedPort + ") is not available.");
            }
            _serialPort = new SerialPort(formattedPort, _baudRate, _parity, _dataBits, _stopBits);
            Debug.Log("[UGS] Port " + formattedPort + " opened.");
        }

        static string FormatPortName(string name)
        {
            string formattedPort = name;

#if UNITY_STANDALONE_WIN
            formattedPort = "\\\\\\\\.\\\\" + name;
#endif
            return formattedPort;
        }

        protected override IEnumerator Read()
        {

            while (isOpen)
            {
                try
                {
                    string data = "";
                    while (_serialPort.BytesToRead > 0)
                    {
                        int b = _serialPort.ReadByte();
                        char c = (char)b;
                        onByteRecieved.Invoke(c);

                        data += c;

                        if (b == Constants.CARRIAGE_RETURN || b == Constants.LINE_FEED)
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
                                _inputLine = line;
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
                    Debug.LogError("[UGS] Read timed out");
                }
                catch (InvalidOperationException)
                {
                    Debug.LogError("[UGS] The port is not open");
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

        void WriteSerial(byte[] data)
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
                    Debug.LogError("[UGS] Write timed out");
                }
            }
        }

        public override void Write(string writeString) {
            byte[] bytes = Encoding.ASCII.GetBytes(writeString);
            WriteSerial(bytes);
        }

        public override void WriteLine(string writeString)
        {

            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.WriteLine(writeString);
                }
                catch (TimeoutException)
                {
                    Debug.LogError("[UGS] Write timed out");
                }
            }

        }

        public override void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                StopCoroutine(readRoutine);
                _serialPort.Close();
                _serialPort = null;
                isOpen = false;
                onClose.Invoke();
            }
        }
    }
}
#endif