using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace DomaSize
{

    [IgnoreFirst(1)]
    [DelimitedRecord(";")]
    public class Map
    {
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string Firstname;
        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string Lastname;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string email;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string date;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string id;
        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string mapname;

        [FieldQuoted('"', QuoteMode.OptionalForRead)] 
        public string createdtime;

        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string MapImage;

        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string ThumbnailImage;

        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string BlankMapImage;

        [FieldQuoted('"', QuoteMode.OptionalForRead)]
        public string navn;

        [FieldOptional]
        public string MapSize;
        [FieldOptional]
        public string BlankMapSize;
    }
}
