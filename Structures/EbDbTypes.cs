namespace ExpressBase.Mobile.Structures
{
    public enum EbDbTypes
    {
        //
        // Summary:
        //     A variable-length stream of non-Unicode characters ranging between 1 and 8,000
        //     characters.
        AnsiString = 0,
        //
        // Summary:
        //     A variable-length stream of binary data ranging between 1 and 8,000 bytes.
        Binary = 1,
        //
        // Summary:
        //     An 8-bit unsigned integer ranging in value from 0 to 255.
        Byte = 2,
        //
        // Summary:
        //     A simple type representing Boolean values of true or false.
        Boolean = 3,
        //
        // Summary:
        //     A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63 -1
        //     (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of a currency
        //     unit.
        Currency = 4,
        //
        // Summary:
        //     A type representing a date value.
        Date = 5,
        //
        // Summary:
        //     A type representing a date and time value.
        DateTime = 6,
        //
        // Summary:
        //     A simple type representing values ranging from 1.0 x 10 -28 to approximately
        //     7.9 x 10 28 with 28-29 significant digits.
        Decimal = 7,
        //
        // Summary:
        //     A floating point type representing values ranging from approximately 5.0 x 10
        //     -324 to 1.7 x 10 308 with a precision of 15-16 digits.
        Double = 8,
        //
        // Summary:
        //     A globally unique identifier (or GUID).
        Guid = 9,
        //
        // Summary:
        //     An integral type representing signed 16-bit integers with values between -32768
        //     and 32767.        
        Int16 = 10,
        //
        // Summary:
        //     An integral type representing signed 32-bit integers with values between -2147483648
        //     and 2147483647.
        Int32 = 11,
        //
        // Summary:
        //     An integral type representing signed 64-bit integers with values between -9223372036854775808
        //     and 9223372036854775807.
        Int64 = 12,
        //
        // Summary:
        //     A general type representing any reference or value type not explicitly represented
        //     by another DbType value.
        Object = 13,
        //
        // Summary:
        //     An integral type representing signed 8-bit integers with values between -128
        //     and 127.
        SByte = 14,
        //
        // Summary:
        //     A floating point type representing values ranging from approximately 1.5 x 10
        //     -45 to 3.4 x 10 38 with a precision of 7 digits.
        Single = 15,
        //
        // Summary:
        //     A type representing Unicode character strings.
        String = 16,
        //
        // Summary:
        //     A type representing a SQL Server DateTime value. If you want to use a SQL Server
        //     time value, use System.Data.SqlDbType.Time.
        Time = 17,
        //
        // Summary:
        //     An integral type representing unsigned 16-bit integers with values between 0
        //     and 65535.
        UInt16 = 18,
        //
        // Summary:
        //     An integral type representing unsigned 32-bit integers with values between 0
        //     and 4294967295.
        UInt32 = 19,
        //
        // Summary:
        //     An integral type representing unsigned 64-bit integers with values between 0
        //     and 18446744073709551615.
        UInt64 = 20,
        //
        // Summary:
        //     A variable-length numeric value.
        VarNumeric = 21,
        //
        // Summary:
        //     A fixed-length stream of non-Unicode characters.
        AnsiStringFixedLength = 22,
        //
        // Summary:
        //     A fixed-length string of Unicode characters.
        StringFixedLength = 23,
        //
        // Summary:
        //     A parsed representation of an XML document or fragment.
        Xml = 25,
        //
        // Summary:
        //     Date and time data. Date value range is from January 1,1 AD through December
        //     31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy
        //     of 100 nanoseconds.
        DateTime2 = 26,
        //
        // Summary:
        //     Date and time data with time zone awareness. Date value range is from January
        //     1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999
        //     with an accuracy of 100 nanoseconds. Time zone value range is -14:00 through
        //     +14:00.
        DateTimeOffset = 27,

        Json = 28,

        Bytea =29,

		BooleanOriginal = 30,

        Int = 31,

        VarChar = 32
    }

    public interface IVendorDbTypes
    {
        VendorDbType AnsiString { get; }
        VendorDbType Binary { get; }
        VendorDbType Byte { get; }
        VendorDbType Boolean { get; }
        //VendorDbType Currency { get; }
        VendorDbType Date { get; }
        VendorDbType DateTime { get; }
        VendorDbType Decimal { get; }
        VendorDbType Double { get; }
        //VendorDbType Guid { get; }
        VendorDbType Int16 { get; }
        VendorDbType Int32 { get; }
        VendorDbType Int64 { get; }
        VendorDbType Object { get; }
        //VendorDbType SByte { get; }
        //VendorDbType Single { get; }
        VendorDbType String { get; }
        VendorDbType Time { get; }
        //VendorDbType UInt16 { get; }
        //VendorDbType UInt32 { get; }
        //VendorDbType UInt64 { get; }
        VendorDbType VarNumeric { get; }
        //VendorDbType AnsiStringFixedLength { get; }
        //VendorDbType StringFixedLength { get; }
        //VendorDbType Xml { get; }
        //VendorDbType DateTime2 { get; }
        //VendorDbType DateTimeOffset { get; }
        VendorDbType Json { get; }
        VendorDbType Bytea { get; }
		VendorDbType BooleanOriginal { get; }
		dynamic GetVendorDbType(EbDbTypes e);
        string GetVendorDbText(EbDbTypes e);
        VendorDbType GetVendorDbTypeStruct(EbDbTypes e);
    }

    public struct VendorDbType
    {
        public EbDbTypes EbDbType { get; }
        public dynamic VDbType { get; }
        public string VDbText { get; }

        public VendorDbType(EbDbTypes type, dynamic vDbType,string vDbText)
        {
            this.EbDbType = type;
            this.VDbType = vDbType;
            this.VDbText = vDbText;
        }
    }
}
