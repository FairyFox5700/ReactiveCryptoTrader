using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CryptoTracker.BL.Utlis
{
    public class TradeMessageUnpacker : IMessageUnpacker
    {
        private static Dictionary<string, int> Fields = new Dictionary<string, int>()
        {
            {"TYPE", 0x0}, // hex for binary 0, it is a special case of fields that are always there          TYPE
            {"MARKET", 0x0}, // hex for binary 0, it is a special case of fields that are always there        MARKET
            {
                "FROMSYMBOL", 0x0
            }, // hex for binary 0, it is a special case of fields that are always there     FROM SYMBOL
            {"TOSYMBOL", 0x0}, // hex for binary 0, it is a special case of fields that are always there       TO SYMBOL
            {"FLAGS", 0x0}, // hex for binary 0, it is a special case of fields that are always there          FLAGS
            {"ID", 0x1}, // hex for binary 1                                                                   ID
            {"TIMESTAMP", 0x2}, // hex for binary 10                                                           TIMESTAMP
            {"QUANTITY", 0x4}, // hex for binary 100                                                           QUANTITY
            {"PRICE", 0x8}, // hex for binary 1000                                                              PRICE
            {"TOTAL", 0x10} // hex for binary 10000                                                            TOTAL
        };


        public bool Supports(string messageType)
        {
            return messageType.Equals("0");
        }

        public Dictionary<string, object> Unpack(string message)
        {
            string[] valuesArray = message.Split("~");
            int valuesArrayLenght = valuesArray.Length;
            string mask = valuesArray[valuesArrayLenght - 1];
            int maskInt = int.Parse(mask, NumberStyles.HexNumber);
            Dictionary<string, object> unpackedTrade = new Dictionary<string, object>();
            int[] currentField = { 0 };

            Fields.ToList().ForEach(t => {
                string k = t.Key;
                int v = t.Value;
                if (v == 0)
                {
                    unpackedTrade.Add(k, valuesArray[currentField[0]]);
                    currentField[0]++;
                }
                else if ((maskInt & v) > 0)
                {
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    unpackedTrade.Add(k, float.Parse(valuesArray[currentField[0]].Trim(), NumberStyles.Any, ci));
                    currentField[0]++;
                }
            });

            return unpackedTrade;
        }
    }
}
    