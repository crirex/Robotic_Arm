using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerIO : Singleton<ManagerIO>, InterfaceIO
{
    #region Private members
    [SerializeField]
    private string portName = "COM9";

    [SerializeField]
    private int baudRate = 115200;

    //[SerializeField]
    //private string laptopMac = "A4:C3:F0:9E:F1:4B";

    //[SerializeField]
    //private string robotMac = "00:1B:10:64:4A:99";

    //[SerializeField]
    //private string robotPin = "0000";
    
    private List<InterfaceIO> membersIO = new List<InterfaceIO>();
    #endregion

    #region Public proprieties
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

    public int ReadLenght
    {
        get
        {
            int totalLenght = 0;
            foreach (InterfaceIO memberIO in membersIO)
            {
                if (memberIO.IsAvailable)
                {
                    totalLenght += memberIO.ReadLenght;
                }
            }
            return totalLenght;
        }
    }
    #endregion

    #region Constructors
    private ManagerIO()
    {
        //AddMember(new BluetoothIO(laptopMac, robotMac, robotPin));
        AddMember(new SerialPortIO(portName, baudRate));
        Initialize();
    }
    #endregion

    #region Public methods
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

    public float ReadFloat()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.ReadFloat();
            }
        }
        return 0;
    }

    public double ReadDouble()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.ReadDouble();
            }
        }
        return 0;
    }

    public byte[] Read(int lenght, int offset = 0)
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.Read(lenght, offset);
            }
        }
        return null;
    }

    public byte[] ReadAll()
    {
        foreach (InterfaceIO memberIO in membersIO)
        {
            if (memberIO.IsAvailable)
            {
                return memberIO.ReadAll();
            }
        }
        return null;
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
    #endregion
}
