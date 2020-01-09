using System;
using System.Globalization;
using FileHelpers;

namespace DomaSize
{
    [DelimitedRecord(";")]
    public class FileZillaLine
    {
        public string DateAndType;

        [FieldConverter(typeof(FileSizeConverter))]
        public long Size;
        [FieldConverter(typeof(UtcTimeConverter))]
        public DateTime Modified;
        public string Mode;
        public string UId;
        public string GId;
        public string Unique;
        [FieldTrim(TrimMode.Both)]
        public string Name;

        public class UtcTimeConverter : ConverterBase
        {
            private const string Format = "yyyyMMddHHmmss";

            public override object StringToField(string from)
            {
                var withoutFractions = from.Substring(from.IndexOf('=') + 1);
                return DateTime.ParseExact(withoutFractions, Format, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            }

            public override string FieldToString(object fieldValue)
            {
                return ((DateTime)fieldValue).ToString(Format);
            }
        }

        public class FileSizeConverter : ConverterBase
        {
            public override object StringToField(string from)
            {
                var value = from.Substring(from.IndexOf('=') + 1);
                return long.Parse(value);
            }

            public override string FieldToString(object fieldValue)
            {
                return fieldValue.ToString();
            }
        }

        [FieldOptional] 
        [FieldNullValue(typeof(bool), "false")]
        public bool IsConnectedToMap;
    }
}
