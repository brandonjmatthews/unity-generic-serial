/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018
 */

using UnityEngine.Events;
using System;

[System.Serializable]
public class SerialDataEvent : UnityEvent<string> {}
[System.Serializable]
public class SerialCharEvent : UnityEvent<char> {}
[System.Serializable]
public class SerialOpenEvent : UnityEvent {}
[System.Serializable]
public class SerialCloseEvent : UnityEvent {}

