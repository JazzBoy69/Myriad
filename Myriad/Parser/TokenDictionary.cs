﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public static class TokenDictionary
    {
        static Dictionary<char, byte> tokenTable = new Dictionary<char, byte>()
        {
            { '-', 2 },
            { ',', 3 },
            { ';', 4 },
            { ':', 5 },
            { '!', 6 },
            { '.', 7 },
            { ' ', 8 }
        };
        static Dictionary<int, int> table = new Dictionary<int, int>()
        {//Key: Alpha or number (A or 1) || nextToLastToken (0 if doesn't matter) || last token || token
            //1 to add citation to results; 0 continue || index to set
            { 0xA448, 0x00 }, //Set first book ; Mt 24:14 => ; ; Mt              
            { 0xA488, 0x00 }, //Set first book ; Mt 24:14 => ; ; Mt             
            { 0xA048, 0x00 }, //Set first book ; Mt 24:14 => ; Mt 
            { 0x1485, 0x01 }, //Set first chapter Mt 24:14 => ;xx _24:            
            { 0x1085, 0x01 }, //Set first chapter Mt 24:14 =>  _24:
            { 0x1484, 0x11 }, //Set first chapter and add results Mt 24; => ;xx_24;            
            { 0x1084, 0x11 }, //Set first chapter and add results Mt 24; =>  _24;
            { 0x1525, 0x05 }, //Set second chapter 2Co 6:14-7:1 => :xx-7:            
            { 0x1025, 0x05 }, //Set second chapter 2Co 6:14-7:1 => -7:
            { 0x1853, 0x02 }, //Set first verse Mt 28:19, 20 => _xx:19,
            { 0x1453, 0x02 }, //Set first verse Mt 24:14; 28:19, 20 => ; xx:19,
            { 0x1854, 0x12 }, //Set first verse and add results Mt 24:14; => _xx:14;
            { 0x1454, 0x12 }, //Set first verse and add results Mt 24:14; 28:20; => ; xx:20;
            { 0x1534, 0x16 }, //Set second verse and add results Re 16:14, 16; => :xx, 16;
            { 0x1852, 0x02 }, //Set first verse Mt 24:45-47 => _xx:45-            
            { 0x1052, 0x02 }, //Set first verse Mt 24:45-47 => :45-            
            { 0x1452, 0x02 }, //Set first verse Mt 6:33; 24:45-47 => ;xx:45-
            { 0x1524, 0x16 }, //Set second verse Mt 24:45-47 => :xx-47;
            { 0x1856, 0x12 }, //Set first verse and add results Mr 2:1! => _xx:1!
            { 0x1456, 0x12 }, //Set first verse and add results Mr 2:1! => ; xx:1!
            { 0x1056, 0x12 }  //Set first verse and add results Mr 2:1! => :1!  
        };

        public static int Lookup(char nextToLastToken, char lastToken, char token, int count)
        {
            int key = (count == Result.notfound) ?
                40960 + 
                256 * tokenTable[nextToLastToken]  + 
                16 * tokenTable[lastToken] + 
                tokenTable[token] 
                :
                4096 + 
                256 * tokenTable[nextToLastToken] + 
                16 * tokenTable[lastToken] + 
                tokenTable[token];
            int wildcardKey = (count == Result.notfound) ?
                40960 +
                16 * tokenTable[lastToken] + 
                tokenTable[token] 
                :
                4096 +
                16 * tokenTable[lastToken] + 
                tokenTable[token];
            if (!table.ContainsKey(key))
            {
                return (table.ContainsKey(wildcardKey)) ?
                table[wildcardKey] : Result.notfound;
            }
            return table[key];
        }
    }
}
