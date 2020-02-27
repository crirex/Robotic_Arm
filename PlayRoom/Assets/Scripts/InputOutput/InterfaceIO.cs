public interface InterfaceIO
{
    /// <summary>
    /// Connection is open
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Is available to read
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Number of bytes to read
    /// </summary>
    int ReadLenght { get; }

    /// <summary>
    /// Write a bytes with a character
    /// </summary>
    void Write(char character);

    /// <summary>
    /// Write more bytes with a list of characters
    /// </summary>
    void Write(string characters);

    /// <summary>
    /// Write a byte
    /// </summary>
    void Write(byte sentByte);

    /// <summary>
    /// Write more bytes
    /// </summary>
    void Write(byte[] sentBytes);

    /// <summary>
    /// Write a byte with an int
    /// </summary>
    void Write(int number);

    /// <summary>
    /// Read the first byte
    /// </summary>
    int Read();

    /// <summary>
    /// Full controll read for bytes
    /// </summary>
    byte[] Read(int lenght, int offset = 0);

    /// <summary>
    /// Read everything on the stream
    /// </summary>
    byte[] ReadAll();

    /// <summary>
    /// Connect devices
    /// </summary>
    void Initialize();

    /// <summary>
    /// Close connection
    /// </summary>
    void Close();
}
