 #define CONNECTIVITY_SOCKET
 
 using System;
 using System.IO;
 using System.Net.Sockets; 
 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 
 namespace Connectivity {
    public class NetConnection : Connection {
    
        TcpClient _netSocket;
        public NetworkStream _netStream;
        StreamWriter _netWriter;
        StreamReader _netReader;
        public String hostName = "INSERT the public IP of router or Local IP of Arduino";
        public Int32 port = 5001; 

        public override void Open() {
            SetupSocket();
        }
        
        void SetupSocket() {
            try {                
                _netSocket = new TcpClient(hostName, port);
                _netStream = _netSocket.GetStream();
                _netWriter = new StreamWriter(_netStream);
                _netReader = new StreamReader(_netStream);
                isOpen = true;
            }
            catch (Exception e) {
                Debug.Log("Socket error:" + e);
            }
        }

        public override void Write(string writeString) {
            if (!isOpen)
                return;
            String tmpString = writeString;
            _netWriter.Write(tmpString);
            _netWriter.Flush();
        }
        
        public override void WriteLine(string writeString) {
            if (!isOpen)
                return;
            String tmpString = writeString;
            _netWriter.WriteLine(tmpString);
            _netWriter.Flush();
        }

        protected override IEnumerator Read() {
            while (isOpen)
            {
                try
                {
                    string data = "";
                    while (_netStream.DataAvailable) {
                        int b = _netStream.ReadByte();
                        char c = (char)b;

                        onByteRecieved.Invoke(c);

                        data += c;

                        if (b == Constants.CARRIAGE_RETURN || b == Constants.LINE_FEED) {
                            // End of line, parse int array and invoke
                            if (this.readLineBuffer.Count > 0) {
                                string line = "";
                                foreach(int i in this.readLineBuffer) {
                                    line += (char)i;
                                }

                                this.readLineBuffer = new List<int>();
                                onLineRecieved.Invoke(line);
                                _inputLine = line;
                            }
                        } else {
                            this.readLineBuffer.Add(b);
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
                    yield return new WaitUntil(() => _netStream.DataAvailable);
                }
                else
                {
                    // Default
                    yield return new WaitForEndOfFrame();
                }

            }
        }
        
        public override void Close() {
            if (!isOpen)
                return;
            _netWriter.Close();
            _netReader.Close();
            _netSocket.Close();
            isOpen = false;
        }

        public override void Flush() {
            if (!isOpen) return;
            _netReader.DiscardBufferedData();
            _netWriter.Flush();
        }
    }
 }