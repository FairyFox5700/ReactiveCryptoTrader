using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CryptoTracker.BL.Utlis
{
    public class PriceMessageUnpacker : IMessageUnpacker
    {
        private Dictionary<string, int> Fields = new Dictionary<string, int>()
        {

            {"TYPE", 0x0}, // hex for binary 0, it is a special case of fields that are always there
            {"MARKET", 0x0}, // hex for binary 0, it is a special case of fields that are always there
            {"FROMSYMBOL", 0x0}, // hex for binary 0, it is a special case of fields that are always there
            {"TOSYMBOL", 0x0}, // hex for binary 0, it is a special case of fields that are always there
            {"FLAGS", 0x0}, // hex for binary 0, it is a special case of fields that are always there
            {"PRICE", 0x1}, // hex for binary 1
            {"BID", 0x2}, // hex for binary 10
            {"OFFER", 0x4}, // hex for binary 100
            {"LASTUPDATE", 0x8}, // hex for binary 1000
            {"AVG", 0x10}, // hex for binary 10000
            {"LASTVOLUME", 0x20}, // hex for binary 100000
            {"LASTVOLUMETO", 0x40}, // hex for binary 1000000
            {"LASTTRADEID", 0x80}, // hex for binary 10000000
            {"VOLUMEHOUR", 0x100}, // hex for binary 100000000
            {"VOLUMEHOURTO", 0x200}, // hex for binary 1000000000
            {"VOLUME24HOUR", 0x400}, // hex for binary 10000000000
            {"VOLUME24HOURTO", 0x800}, // hex for binary 100000000000
            {"OPENHOUR", 0x1000}, // hex for binary 1000000000000
            {"HIGHHOUR", 0x2000}, // hex for binary 10000000000000
            {"LOWHOUR", 0x4000}, // hex for binary 100000000000000
            {"OPEN24HOUR", 0x8000}, // hex for binary 1000000000000000
            {"HIGH24HOUR", 0x10000}, // hex for binary 10000000000000000
            {"LOW24HOUR", 0x20000}, // hex for binary 100000000000000000
            {
                "LASTMARKET", 0x40000
            } // hex for binary 1000000000000000000, this is a special case and will only appear on CCCAGG messages
        };


        public bool Supports(string messageType)
        {
            return messageType.Equals("5");
        }

        public Dictionary<string, object> Unpack(string message)
        {
            string[] valuesArray = message.Split("~");
            int valuesArrayLenght = valuesArray.Length;
            string mask = valuesArray[valuesArrayLenght - 1];
            int maskInt = int.Parse(mask, NumberStyles.HexNumber);
            Dictionary<string, object> unpackedCurrent = new Dictionary<string, object>();
            int[] currentField = { 0 };
            Fields.ToList().ForEach(t => {
                string k = t.Key;
                int v = t.Value;
                if (v == 0)
                {
                    unpackedCurrent.Add(k, valuesArray[currentField[0]]);
                    currentField[0]++;
                }
                else if ((maskInt & v) > 0)
                {
                    //i know this is a hack, for cccagg, future code please don't hate me:(, i did this to avoid
                    //subscribing to trades as well in order to show the last market
                    if (k.Equals("LASTMARKET") || k.Equals("LASTTRADEID"))
                    {
                        unpackedCurrent.Add(k, valuesArray[currentField[0]]);
                    }
                    else
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";
                        unpackedCurrent.Add(k, float.Parse(valuesArray[currentField[0]].Trim(), NumberStyles.Any, ci));
                    }
                    currentField[0]++;
                }
            });

            return unpackedCurrent;
        }
    }
}