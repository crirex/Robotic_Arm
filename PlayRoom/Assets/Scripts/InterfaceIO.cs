public interface InterfaceIO
{
    // Connection is open
    bool IsOpen { get; }

    // Is available to read
    bool IsAvailable { get; }

    // Write a bytes with a character
    void Write(char character);

    // Write more bytes with a list of characters
    void Write(string characters);

    // Write a byte
    void Write(byte sentByte);

    // Write more bytes
    void Write(byte[] sentBytes);

    // Write a byte with an int
    void Write(int number);

    //Read the first byte
    int Read();

    //Full controll read for bytes
    int Read(int lenght, int offset = 0);

    // Read everything on the stream
    int ReadAll();

    // Connect devices
    void Initialize();

    // Close connection
    void Close();
}
