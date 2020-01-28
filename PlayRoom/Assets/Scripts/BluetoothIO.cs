using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using UnityEngine;

public class BluetoothIO : InterfaceIO
{
    private readonly BluetoothEndPoint EP;
    private readonly BluetoothClient BC;
    private readonly BluetoothDeviceInfo BTDevice;

    private string UserMacAdress {get; set;}

    private string DeviceMacAdress { get; set; }

    private string Pin { get; set; }

    public bool IsOpen => BC.GetStream().CanRead ? BC != null : false;

    public bool IsAvailable => BC.GetStream().DataAvailable ? BC != null : false;

    public BluetoothIO(string userMacAdress, string deviceMacAdress, string pin = "1234")
    {
        this.UserMacAdress = userMacAdress;
        this.DeviceMacAdress = deviceMacAdress;
        this.Pin = pin;
        this.EP = new BluetoothEndPoint(BluetoothAddress.Parse(this.UserMacAdress), BluetoothService.BluetoothBase);
        this.BC = new BluetoothClient(this.EP);
        this.BTDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(this.DeviceMacAdress));
    }

    public void Close()
    {
        BC.Close();
    }

    public void Initialize()
    {
        Debug.Log("Started: YES");
        if (BluetoothSecurity.PairRequest(BTDevice.DeviceAddress, this.Pin))
        {
            Debug.Log("PairRequest: OK");

            if (BTDevice.Authenticated)
            {
                Debug.Log("Authenticated: OK");
                BC.SetPin(this.Pin);
                BC.BeginConnect(BTDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), BTDevice);
                if (IsOpen)
                {
                    Debug.Log("You can read from this NetworkStream.");
                }
                else
                {
                    Debug.Log("Sorry. You cannot read from this NetworkStream.");
                }
            }
            else
            {
                Debug.Log("Authenticated: No");
            }
        }
        Debug.Log("PairRequest: No");
    }

    private static void Connect(IAsyncResult result)
    {
        if (result.IsCompleted)
        {
            Debug.Log("Client is connected now.");
        }
        Debug.Log("Client is not connected");
    }

    public int Read()
    {
        if (IsOpen && IsAvailable)
        {
            return BC.GetStream().ReadByte();
        }
        else
        {
            Debug.Log("Sorry. You cannot read from this NetworkStream.");
            return 0;
        }
    }

    public int Read(int lenght, int offset = 0)
    {
        if (IsOpen && IsAvailable)
        {
            byte[] readBytes = new byte[lenght];
            return BC.GetStream().Read(readBytes, offset, readBytes.Length);
        }
        else
        {
            Debug.Log("Sorry. You cannot read from this NetworkStream.");
            return 0;
        }
    }

    public int ReadAll()
    {
        if (IsOpen && IsAvailable)
        {
            byte[] readBytes = new byte[BC.GetStream().Length];
            return BC.GetStream().Read(readBytes, 0, readBytes.Length);
        }
        else
        {
            Debug.Log("Sorry. You cannot read from this NetworkStream.");
            return 0;
        }
    }

    public void Write(char character)
    {
        if (IsOpen)
        {
            BC.GetStream().WriteByte((byte)character);
        }
        else
        {
            Debug.Log("Sorry. You cannot write on this NetworkStream.");
        }
    }

    public void Write(string characters)
    {
        if (IsOpen)
        {
            foreach(char character in characters)
            {
                BC.GetStream().WriteByte((byte)character);
            }
        }
        else
        {
            Debug.Log("Sorry. You cannot write on this NetworkStream.");
        }
    }

    public void Write(byte sentByte)
    {
        if (IsOpen)
        {
            BC.GetStream().WriteByte(sentByte);
        }
        else
        {
            Debug.Log("Sorry. You cannot write on this NetworkStream.");
        }
    }

    public void Write(byte[] sentBytes)
    {
        if (IsOpen)
        {
            BC.GetStream().Write(sentBytes, 0, sentBytes.Length);
        }
        else
        {
            Debug.Log("Sorry. You cannot write on this NetworkStream.");
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
            BC.GetStream().Write(intBytes, 0, intBytes.Length);
        }
        else
        {
            Debug.Log("Sorry. You cannot write on this NetworkStream.");
        }
    }
}
