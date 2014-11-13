
internal static class StringHelper
{

    internal static string NewString(byte[] bytes)
    {
        return NewString(bytes, 0, bytes.Length);
    }
    internal static string NewString(byte[] bytes, int index, int count)
    {
        return System.Text.Encoding.UTF8.GetString((byte[])(object)bytes, index, count);
    }
    internal static string NewString(sbyte[] bytes, string encoding)
    {
        return NewString(bytes, 0, bytes.Length, encoding);
    }
    internal static string NewString(sbyte[] bytes, int index, int count, string encoding)
    {
        return System.Text.Encoding.GetEncoding(encoding).GetString((byte[])(object)bytes, index, count);
    }

    //--------------------------------------------------------------------------------
    //	These methods are used to replace calls to the Java String.GetBytes methods.
    //--------------------------------------------------------------------------------
    internal static sbyte[] GetBytes(this string self)
    {
        return GetSBytesForEncoding(System.Text.Encoding.UTF8, self);
    }
    internal static sbyte[] GetBytes(this string self, string encoding)
    {
        return GetSBytesForEncoding(System.Text.Encoding.GetEncoding(encoding), self);
    }
    private static sbyte[] GetSBytesForEncoding(System.Text.Encoding encoding, string s)
    {
        sbyte[] sbytes = new sbyte[encoding.GetByteCount(s)];
        encoding.GetBytes(s, 0, s.Length, (byte[])(object)sbytes, 0);
        return sbytes;
    }
}