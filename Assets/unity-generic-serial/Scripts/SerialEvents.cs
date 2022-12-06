/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018
 */

using UnityEngine.Events;
using System;

namespace Connectivity
{
    [System.Serializable]
    public class DataEvent : UnityEvent<string> { }
    [System.Serializable]
    public class CharEvent : UnityEvent<char> { }
    [System.Serializable]
    public class OpenEvent : UnityEvent { }
    [System.Serializable]
    public class CloseEvent : UnityEvent { }
}

