/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018

Simple demo of how to use SerialConnection
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

	public void ByteRecieved(char c) {
		//Debug.Log("CHAR: " + c);
	}

	public void LineRecieved(string line) {
		Debug.Log("LINE: " + line);
	}

	public void DataRecieved(string data) {
		//Debug.Log("DATA: " + data);
	}

	public void SerialOpened() {
		Debug.Log("SERIAL OPENED");
	}

	
	public void SerialClosed() {
		Debug.Log("SERIAL CLOSED");
	}
}
