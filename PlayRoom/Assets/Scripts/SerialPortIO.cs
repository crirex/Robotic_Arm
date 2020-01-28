﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class SerialPortIO : InterfaceIO
{
    private SerialPort serial;

    public bool IsOpen => serial.IsOpen;

    public bool IsAvailable => serial.ReadBufferSize > 0 ? serial != null : false;

    public SerialPortIO(string portName, int baudRate)
    {
        this.serial = new SerialPort(portName, baudRate);
        this.serial.ReadTimeout = 1;
    }

    public void Close()
    {
        serial.Close();
    }

    public void Initialize()
    {
        try
        {
            serial.Open();
            Debug.Log("Serial opened.");
        }
        catch 
        {
            Debug.Log("Serial failed to open.");
        }
    }

    public int Read()
    {
        if(IsOpen & IsAvailable)
        {
            return serial.ReadChar();
        }
        else
        {
            Debug.Log("Sorry, you can not read from serial.");
            return 0;
        }
    }

    public int Read(int lenght, int offset = 0)
    {
        if (IsOpen & IsAvailable)
        {
            byte[] bytes = new byte[lenght];
            return serial.Read(bytes, offset, bytes.Length);
        }
        else
        {
            Debug.Log("Sorry, you can not read from serial.");
            return 0;
        }
    }

    public int ReadAll()
    {
        if (IsOpen & IsAvailable)
        {
            byte[] bytes = new byte[serial.ReadBufferSize];
            return serial.Read(bytes, 0, bytes.Length);
        }
        else
        {
            Debug.Log("Sorry, you can not read from serial.");
            return 0;
        }
    }

    public void Write(char character)
    {
        if (IsOpen)
        {
            serial.Write(character+"");
        }
        else
        {
            Debug.Log("Sorry, you can not write to serial.");
        }
    }

    public void Write(string characters)
    {
        if (IsOpen)
        {
            serial.Write(characters);
        }
        else
        {
            Debug.Log("Sorry, you can not write to serial.");
        }
    }

    public void Write(byte sentByte)
    {
        if (IsOpen)
        {
            serial.Write(new byte[1] { sentByte }, 0, 1);
        }
        else
        {
            Debug.Log("Sorry, you can not write to serial.");
        }
    }

    public void Write(byte[] sentBytes)
    {
        if (IsOpen)
        {
            serial.Write(sentBytes, 0, sentBytes.Length);
        }
        else
        {
            Debug.Log("Sorry, you can not write to serial.");
        }
    }

    public void Write(int number)
    {
        if (IsOpen)
        {
            byte[] intBytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(intBytes);
            }
            serial.Write(intBytes, 0, intBytes.Length);
        }
        else
        {
            Debug.Log("Sorry, you can not write to serial.");
        }
    }
}