#define CONNECTIVITY

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
using System.Text;
using UnityEngine;
namespace Connectivity
{
    public abstract class Connection : MonoBehaviour
    {
        public CharEvent onByteRecieved;
        public DataEvent onDataRecieved;
        public DataEvent onLineRecieved;
        public OpenEvent onOpen;
        public CloseEvent onClose;
        public WaitFor waitFor = WaitFor.EndOfFrame;

        public bool openOnStart = false;
        protected string _inputLine;
        protected int _readTimeout = 200;
        protected int _writeTimeout = 2000;

        protected Coroutine inputRoutine;

        protected List<int> readLineBuffer = new List<int>();

        public bool isOpen { get; protected set; } = false;

        void Start()
        {
            if (openOnStart) {
                Open();
            }
        }

        public abstract void Flush();

        public abstract void Open();

        protected abstract IEnumerator Read();

        // public abstract void Write(byte[] data);

        public abstract void Write(string writeString);

        public abstract void WriteLine(string writeString);

        void OnApplicationQuit()
        {
            Close();
        }

        public abstract void Close();
    }
}