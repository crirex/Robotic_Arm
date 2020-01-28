using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerIO : Singleton<ManagerIO>, InterfaceIO
{
    [SerializeField]
    private string portName = "COM5";

    [SerializeField]
    private int baudRate = 115200;

    [SerializeField]
    private string laptopMac = "A4:C3:F0:9E:F1:4B";

    [SerializeField]
    private string robotMac = "00:1B:10:64:4A:99";

    [SerializeField]
    private string robotPin = "0000";

    private List<InterfaceIO> membersIO = new List<InterfaceIO>();

    public bool IsOpen
    {
        get
        {
            foreach (InterfaceIO memberIO in membersIO)
            {
                if(memberIO.IsOpen)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool IsAvailable
    {
        get
        {
            foreach (InterfaceIO memberIO in membersIO)
            {
                if (memberIO.IsAvailable)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AddMember(new BluetoothIO(laptopMac, robotMac, robotPin));
        AddMember(new SerialPortIO(portName, baudRate));
        Initialize();
    }

    public void AddMember(InterfaceIO newMemberIO)
    {
        membersIO.Add(newMemberIO);
    }

    public void Write(char character)
    {
        foreach(InterfaceIO memberIO in membersIO)
        {
            memberIO.Write(character);
        }
    }

    public void Write(string characters)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Write(characters);
        }
    }

    public void Write(byte sentByte)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Write(sentByte);
        }
    }

    public void Write(byte[] sentBytes)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Write(sentBytes);
        }
    }

    public void Write(int number)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Write(number);
        }
    }

    public int Read()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if(memberIO.IsAvailable)
            {
                return memberIO.Read();
            }
        }
        return 0;
    }

    public int Read(int lenght, int offset = 0)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.Read(lenght, offset);
            }
        }
        return 0;
    }

    public int ReadAll()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.ReadAll();
            }
        }
        return 0;
    }

    public void Initialize()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Initialize();
        }
    }

    public void Close()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            memberIO.Close();
        }
    }
}
