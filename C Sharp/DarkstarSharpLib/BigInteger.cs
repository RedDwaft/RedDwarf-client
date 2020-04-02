using System;
using System.Collections.Generic;

namespace DarkstarSharp
{
    //************************************************************************************
    // BigInteger Class Version 1.03
    //
    // Copyright (c) 2002 Chew Keong TAN
    // All rights reserved.
    //
    // Permission is hereby granted, free of charge, to any person obtaining a
    // copy of this software and associated documentation files (the
    // "Software"), to deal in the Software without restriction, including
    // without limitation the rights to use, copy, modify, merge, publish,
    // distribute, and/or sell copies of the Software, and to permit persons
    // to whom the Software is furnished to do so, provided that the above
    // copyright notice(s) and this permission notice appear in all copies of
    // the Software and that both the above copyright notice(s) and this
    // permission notice appear in supporting documentation
    //

    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
    // OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
    // OF THIRD PARTY RIGHTS. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    // HOLDERS INCLUDED IN THIS NOTICE BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL
    // INDIRECT OR CONSEQUENTIAL DAMAGES, OR ANY DAMAGES WHATSOEVER RESULTING
    // FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT,
    // NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION
    // WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
    //

    //

    // Disclaimer
    // ----------
    // Although reasonable care has been taken to ensure the correctness of this
    // implementation, this code should never be used in any application without
    // proper verification and testing.  I disclaim all liability and responsibility
    // to any person or entity with respect to any loss or damage caused, or alleged
    // to be caused, directly or indirectly, by the use of this BigInteger class.
    //

    // Comments, bugs and suggestions to
    // (http://www.codeproject.com/csharp/biginteger.asp)
    //
    //
    //************************************************************************************



    using System;





    public class BigInteger
    {
        // maximum length of the BigInteger in uint (4 bytes)
        // change this to suit the required level of precision.
        private const int maxLength = 70;
        // primes smaller than 2000 to test the generated prime number

        public static readonly int[] primesBelow2000 = {

        2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,

        101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,

	211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293,

	307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,

	401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499,

	503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599,

	601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691,

	701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797,

	809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887,

	907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,

	1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097,

	1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193,

	1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297,

	1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399,

	1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499,

	1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597,

	1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699,

	1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789,

	1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,

	1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999 };





        private uint[] data = null;             // stores bytes from the Big Integer

        public int dataLength;                 // number of actual chars used





        //***********************************************************************

        // Constructor (Default value for BigInteger is 0

        //***********************************************************************

        public BigInteger()
        {
            data = new uint[maxLength];
            dataLength = 1;
        }

        //***********************************************************************
        // Constructor (Default value provided by long)
        //***********************************************************************

        public BigInteger(long value)
        {

            data = new uint[maxLength];

            long tempVal = value;



            // copy bytes from long to BigInteger without any assumption of

            // the length of the long datatype



            dataLength = 0;

            while (value != 0 && dataLength < maxLength)
            {

                data[dataLength] = (uint)(value & 0xFFFFFFFF);

                value >>= 32;

                dataLength++;

            }



            if (tempVal > 0)         // overflow check for +ve value
            {

                if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)

                    throw (new ArithmeticException("Positive overflow in constructor."));

            }

            else if (tempVal < 0)    // underflow check for -ve value
            {

                if (value != -1 || (data[dataLength - 1] & 0x80000000) == 0)

                    throw (new ArithmeticException("Negative underflow in constructor."));

            }



            if (dataLength == 0)

                dataLength = 1;

        }





        //***********************************************************************

        // Constructor (Default value provided by ulong)

        //***********************************************************************



        public BigInteger(ulong value)
        {

            data = new uint[maxLength];



            // copy bytes from ulong to BigInteger without any assumption of

            // the length of the ulong datatype



            dataLength = 0;

            while (value != 0 && dataLength < maxLength)
            {

                data[dataLength] = (uint)(value & 0xFFFFFFFF);

                value >>= 32;

                dataLength++;

            }



            if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)

                throw (new ArithmeticException("Positive overflow in constructor."));



            if (dataLength == 0)

                dataLength = 1;

        }







        //***********************************************************************

        // Constructor (Default value provided by BigInteger)

        //***********************************************************************



        public BigInteger(BigInteger bi)
        {

            data = new uint[maxLength];



            dataLength = bi.dataLength;



            for (int i = 0; i < dataLength; i++)

                data[i] = bi.data[i];

        }





        //***********************************************************************

        // Constructor (Default value provided by a string of digits of the

        //              specified base)

        //

        // Example (base 10)

        // -----------------

        // To initialize "a" with the default value of 1234 in base 10

        //      BigInteger a = new BigInteger("1234", 10)

        //

        // To initialize "a" with the default value of -1234

        //      BigInteger a = new BigInteger("-1234", 10)

        //

        // Example (base 16)

        // -----------------

        // To initialize "a" with the default value of 0x1D4F in base 16

        //      BigInteger a = new BigInteger("1D4F", 16)

        //

        // To initialize "a" with the default value of -0x1D4F

        //      BigInteger a = new BigInteger("-1D4F", 16)

        //

        // Note that string values are specified in the <sign><magnitude>

        // format.

        //

        //***********************************************************************



        public BigInteger(string value, int radix)
        {

            BigInteger multiplier = new BigInteger(1);

            BigInteger result = new BigInteger();

            value = (value.ToUpper()).Trim();

            int limit = 0;



            if (value[0] == '-')

                limit = 1;



            for (int i = value.Length - 1; i >= limit; i--)
            {

                int posVal = (int)value[i];



                if (posVal >= '0' && posVal <= '9')

                    posVal -= '0';

                else if (posVal >= 'A' && posVal <= 'Z')

                    posVal = (posVal - 'A') + 10;

                else

                    posVal = 9999999;       // arbitrary large





                if (posVal >= radix)

                    throw (new ArithmeticException("Invalid string in constructor."));

                else
                {

                    if (value[0] == '-')

                        posVal = -posVal;



                    result = result + (multiplier * posVal);



                    if ((i - 1) >= limit)

                        multiplier = multiplier * radix;

                }

            }



            if (value[0] == '-')     // negative values
            {

                if ((result.data[maxLength - 1] & 0x80000000) == 0)

                    throw (new ArithmeticException("Negative underflow in constructor."));

            }

            else    // positive values
            {

                if ((result.data[maxLength - 1] & 0x80000000) != 0)

                    throw (new ArithmeticException("Positive overflow in constructor."));

            }



            data = new uint[maxLength];

            for (int i = 0; i < result.dataLength; i++)

                data[i] = result.data[i];



            dataLength = result.dataLength;

        }





        //***********************************************************************

        // Constructor (Default value provided by an array of bytes)

        //

        // The lowest index of the input byte array (i.e [0]) should contain the

        // most significant byte of the number, and the highest index should

        // contain the least significant byte.

        //

        // E.g.

        // To initialize "a" with the default value of 0x1D4F in base 16

        //      byte[] temp = { 0x1D, 0x4F };

        //      BigInteger a = new BigInteger(temp)

        //

        // Note that this method of initialization does not allow the

        // sign to be specified.

        //

        //***********************************************************************



        public BigInteger(byte[] inData)
        {

            dataLength = inData.Length >> 2;



            int leftOver = inData.Length & 0x3;

            if (leftOver != 0)         // length not multiples of 4

                dataLength++;





            if (dataLength > maxLength)

                throw (new ArithmeticException("Byte overflow in constructor."));



            data = new uint[maxLength];



            for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++)
            {

                data[j] = (uint)((inData[i - 3] << 24) + (inData[i - 2] << 16) +

                                 (inData[i - 1] << 8) + inData[i]);

            }



            if (leftOver == 1)

                data[dataLength - 1] = (uint)inData[0];

            else if (leftOver == 2)

                data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);

            else if (leftOver == 3)

                data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);





            while (dataLength > 1 && data[dataLength - 1] == 0)

                dataLength--;



            //Console.WriteLine("Len = " + dataLength);

        }





        //***********************************************************************

        // Constructor (Default value provided by an array of bytes of the

        // specified length.)

        //***********************************************************************



        public BigInteger(byte[] inData, int inLen)
        {

            dataLength = inLen >> 2;



            int leftOver = inLen & 0x3;

            if (leftOver != 0)         // length not multiples of 4

                dataLength++;



            if (dataLength > maxLength || inLen > inData.Length)

                throw (new ArithmeticException("Byte overflow in constructor."));





            data = new uint[maxLength];



            for (int i = inLen - 1, j = 0; i >= 3; i -= 4, j++)
            {

                data[j] = (uint)((inData[i - 3] << 24) + (inData[i - 2] << 16) +

                                 (inData[i - 1] << 8) + inData[i]);

            }



            if (leftOver == 1)

                data[dataLength - 1] = (uint)inData[0];

            else if (leftOver == 2)

                data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);

            else if (leftOver == 3)

                data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);





            if (dataLength == 0)

                dataLength = 1;



            while (dataLength > 1 && data[dataLength - 1] == 0)

                dataLength--;



            //Console.WriteLine("Len = " + dataLength);

        }





        //***********************************************************************

        // Constructor (Default value provided by an array of unsigned integers)

        //*********************************************************************



        public BigInteger(uint[] inData)
        {

            dataLength = inData.Length;



            if (dataLength > maxLength)

                throw (new ArithmeticException("Byte overflow in constructor."));



            data = new uint[maxLength];



            for (int i = dataLength - 1, j = 0; i >= 0; i--, j++)

                data[j] = inData[i];



            while (dataLength > 1 && data[dataLength - 1] == 0)

                dataLength--;



            //Console.WriteLine("Len = " + dataLength);

        }





        //***********************************************************************

        // Overloading of the typecast operator.

        // For BigInteger bi = 10;

        //***********************************************************************



        public static implicit operator BigInteger(long value)
        {

            return (new BigInteger(value));

        }



        public static implicit operator BigInteger(ulong value)
        {

            return (new BigInteger(value));

        }



        public static implicit operator BigInteger(int value)
        {

            return (new BigInteger((long)value));

        }



        public static implicit operator BigInteger(uint value)
        {

            return (new BigInteger((ulong)value));

        }





        //***********************************************************************

        // Overloading of addition operator

        //***********************************************************************



        public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
        {

            BigInteger result = new BigInteger();



            result.dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;



            long carry = 0;

            for (int i = 0; i < result.dataLength; i++)
            {

                long sum = (long)bi1.data[i] + (long)bi2.data[i] + carry;

                carry = sum >> 32;

                result.data[i] = (uint)(sum & 0xFFFFFFFF);

            }



            if (carry != 0 && result.dataLength < maxLength)
            {

                result.data[result.dataLength] = (uint)(carry);

                result.dataLength++;

            }



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;





            // overflow check

            int lastPos = maxLength - 1;

            if ((bi1.data[lastPos] & 0x80000000) == (bi2.data[lastPos] & 0x80000000) &&

               (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            {

                throw (new ArithmeticException());

            }



            return result;

        }





        //***********************************************************************

        // Overloading of the unary ++ operator

        //***********************************************************************



        public static BigInteger operator ++(BigInteger bi1)
        {

            BigInteger result = new BigInteger(bi1);



            long val, carry = 1;

            int index = 0;



            while (carry != 0 && index < maxLength)
            {

                val = (long)(result.data[index]);

                val++;



                result.data[index] = (uint)(val & 0xFFFFFFFF);

                carry = val >> 32;



                index++;

            }



            if (index > result.dataLength)

                result.dataLength = index;

            else
            {

                while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                    result.dataLength--;

            }



            // overflow check

            int lastPos = maxLength - 1;



            // overflow if initial value was +ve but ++ caused a sign

            // change to negative.



            if ((bi1.data[lastPos] & 0x80000000) == 0 &&

               (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            {

                throw (new ArithmeticException("Overflow in ++."));

            }

            return result;

        }





        //***********************************************************************

        // Overloading of subtraction operator

        //***********************************************************************



        public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
        {

            BigInteger result = new BigInteger();



            result.dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;



            long carryIn = 0;

            for (int i = 0; i < result.dataLength; i++)
            {

                long diff;



                diff = (long)bi1.data[i] - (long)bi2.data[i] - carryIn;

                result.data[i] = (uint)(diff & 0xFFFFFFFF);



                if (diff < 0)

                    carryIn = 1;

                else

                    carryIn = 0;

            }



            // roll over to negative

            if (carryIn != 0)
            {

                for (int i = result.dataLength; i < maxLength; i++)

                    result.data[i] = 0xFFFFFFFF;

                result.dataLength = maxLength;

            }



            // fixed in v1.03 to give correct datalength for a - (-b)

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            // overflow check



            int lastPos = maxLength - 1;

            if ((bi1.data[lastPos] & 0x80000000) != (bi2.data[lastPos] & 0x80000000) &&

               (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            {

                throw (new ArithmeticException());

            }



            return result;

        }





        //***********************************************************************

        // Overloading of the unary -- operator

        //***********************************************************************



        public static BigInteger operator --(BigInteger bi1)
        {

            BigInteger result = new BigInteger(bi1);



            long val;

            bool carryIn = true;

            int index = 0;



            while (carryIn && index < maxLength)
            {

                val = (long)(result.data[index]);

                val--;



                result.data[index] = (uint)(val & 0xFFFFFFFF);



                if (val >= 0)

                    carryIn = false;



                index++;

            }



            if (index > result.dataLength)

                result.dataLength = index;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            // overflow check

            int lastPos = maxLength - 1;



            // overflow if initial value was -ve but -- caused a sign

            // change to positive.



            if ((bi1.data[lastPos] & 0x80000000) != 0 &&

               (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            {

                throw (new ArithmeticException("Underflow in --."));

            }



            return result;

        }





        //***********************************************************************

        // Overloading of multiplication operator

        //***********************************************************************



        public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
        {

            int lastPos = maxLength - 1;

            bool bi1Neg = false, bi2Neg = false;



            // take the absolute value of the inputs

            try
            {

                if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
                {

                    bi1Neg = true; bi1 = -bi1;

                }

                if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative
                {

                    bi2Neg = true; bi2 = -bi2;

                }

            }

            catch (Exception) { }



            BigInteger result = new BigInteger();



            // multiply the absolute values

            try
            {

                for (int i = 0; i < bi1.dataLength; i++)
                {

                    if (bi1.data[i] == 0) continue;



                    ulong mcarry = 0;

                    for (int j = 0, k = i; j < bi2.dataLength; j++, k++)
                    {

                        // k = i + j

                        ulong val = ((ulong)bi1.data[i] * (ulong)bi2.data[j]) +

                                     (ulong)result.data[k] + mcarry;



                        result.data[k] = (uint)(val & 0xFFFFFFFF);

                        mcarry = (val >> 32);

                    }



                    if (mcarry != 0)

                        result.data[i + bi2.dataLength] = (uint)mcarry;

                }

            }

            catch (Exception)
            {

                throw (new ArithmeticException("Multiplication overflow."));

            }





            result.dataLength = bi1.dataLength + bi2.dataLength;

            if (result.dataLength > maxLength)

                result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            // overflow check (result is -ve)

            if ((result.data[lastPos] & 0x80000000) != 0)
            {

                if (bi1Neg != bi2Neg && result.data[lastPos] == 0x80000000)    // different sign
                {

                    // handle the special case where multiplication produces

                    // a max negative number in 2's complement.



                    if (result.dataLength == 1)

                        return result;

                    else
                    {

                        bool isMaxNeg = true;

                        for (int i = 0; i < result.dataLength - 1 && isMaxNeg; i++)
                        {

                            if (result.data[i] != 0)

                                isMaxNeg = false;

                        }



                        if (isMaxNeg)

                            return result;

                    }

                }



                throw (new ArithmeticException("Multiplication overflow."));

            }



            // if input has different signs, then result is -ve

            if (bi1Neg != bi2Neg)

                return -result;



            return result;

        }







        //***********************************************************************

        // Overloading of unary << operators

        //***********************************************************************



        public static BigInteger operator <<(BigInteger bi1, int shiftVal)
        {

            BigInteger result = new BigInteger(bi1);

            result.dataLength = shiftLeft(result.data, shiftVal);



            return result;

        }





        // least significant bits at lower part of buffer



        private static int shiftLeft(uint[] buffer, int shiftVal)
        {

            int shiftAmount = 32;

            int bufLen = buffer.Length;



            while (bufLen > 1 && buffer[bufLen - 1] == 0)

                bufLen--;



            for (int count = shiftVal; count > 0; )
            {

                if (count < shiftAmount)

                    shiftAmount = count;



                //Console.WriteLine("shiftAmount = {0}", shiftAmount);



                ulong carry = 0;

                for (int i = 0; i < bufLen; i++)
                {

                    ulong val = ((ulong)buffer[i]) << shiftAmount;

                    val |= carry;



                    buffer[i] = (uint)(val & 0xFFFFFFFF);

                    carry = val >> 32;

                }



                if (carry != 0)
                {

                    if (bufLen + 1 <= buffer.Length)
                    {

                        buffer[bufLen] = (uint)carry;

                        bufLen++;

                    }

                }

                count -= shiftAmount;

            }

            return bufLen;

        }





        //***********************************************************************

        // Overloading of unary >> operators

        //***********************************************************************



        public static BigInteger operator >>(BigInteger bi1, int shiftVal)
        {

            BigInteger result = new BigInteger(bi1);

            result.dataLength = shiftRight(result.data, shiftVal);





            if ((bi1.data[maxLength - 1] & 0x80000000) != 0) // negative
            {

                for (int i = maxLength - 1; i >= result.dataLength; i--)

                    result.data[i] = 0xFFFFFFFF;



                uint mask = 0x80000000;

                for (int i = 0; i < 32; i++)
                {

                    if ((result.data[result.dataLength - 1] & mask) != 0)

                        break;



                    result.data[result.dataLength - 1] |= mask;

                    mask >>= 1;

                }

                result.dataLength = maxLength;

            }



            return result;

        }





        private static int shiftRight(uint[] buffer, int shiftVal)
        {

            int shiftAmount = 32;

            int invShift = 0;

            int bufLen = buffer.Length;



            while (bufLen > 1 && buffer[bufLen - 1] == 0)

                bufLen--;



            //Console.WriteLine("bufLen = " + bufLen + " buffer.Length = " + buffer.Length);



            for (int count = shiftVal; count > 0; )
            {

                if (count < shiftAmount)
                {

                    shiftAmount = count;

                    invShift = 32 - shiftAmount;

                }



                //Console.WriteLine("shiftAmount = {0}", shiftAmount);



                ulong carry = 0;

                for (int i = bufLen - 1; i >= 0; i--)
                {

                    ulong val = ((ulong)buffer[i]) >> shiftAmount;

                    val |= carry;



                    carry = ((ulong)buffer[i]) << invShift;

                    buffer[i] = (uint)(val);

                }



                count -= shiftAmount;

            }



            while (bufLen > 1 && buffer[bufLen - 1] == 0)

                bufLen--;



            return bufLen;

        }





        //***********************************************************************

        // Overloading of the NOT operator (1's complement)

        //***********************************************************************



        public static BigInteger operator ~(BigInteger bi1)
        {

            BigInteger result = new BigInteger(bi1);



            for (int i = 0; i < maxLength; i++)

                result.data[i] = (uint)(~(bi1.data[i]));



            result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            return result;

        }





        //***********************************************************************

        // Overloading of the NEGATE operator (2's complement)

        //***********************************************************************



        public static BigInteger operator -(BigInteger bi1)
        {

            // handle neg of zero separately since it'll cause an overflow

            // if we proceed.



            if (bi1.dataLength == 1 && bi1.data[0] == 0)

                return (new BigInteger());



            BigInteger result = new BigInteger(bi1);



            // 1's complement

            for (int i = 0; i < maxLength; i++)

                result.data[i] = (uint)(~(bi1.data[i]));



            // add one to result of 1's complement

            long val, carry = 1;

            int index = 0;



            while (carry != 0 && index < maxLength)
            {

                val = (long)(result.data[index]);

                val++;



                result.data[index] = (uint)(val & 0xFFFFFFFF);

                carry = val >> 32;



                index++;

            }



            if ((bi1.data[maxLength - 1] & 0x80000000) == (result.data[maxLength - 1] & 0x80000000))

                throw (new ArithmeticException("Overflow in negation.\n"));



            result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;

            return result;

        }





        //***********************************************************************

        // Overloading of equality operator

        //***********************************************************************



        public static bool operator ==(BigInteger bi1, BigInteger bi2)
        {

            return bi1.Equals(bi2);

        }





        public static bool operator !=(BigInteger bi1, BigInteger bi2)
        {

            return !(bi1.Equals(bi2));

        }





        public override bool Equals(object o)
        {

            BigInteger bi = (BigInteger)o;



            if (this.dataLength != bi.dataLength)

                return false;



            for (int i = 0; i < this.dataLength; i++)
            {

                if (this.data[i] != bi.data[i])

                    return false;

            }

            return true;

        }





        public override int GetHashCode()
        {

            return this.ToString().GetHashCode();

        }





        //***********************************************************************

        // Overloading of inequality operator

        //***********************************************************************



        public static bool operator >(BigInteger bi1, BigInteger bi2)
        {

            int pos = maxLength - 1;



            // bi1 is negative, bi2 is positive

            if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)

                return false;



                // bi1 is positive, bi2 is negative

            else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)

                return true;



            // same sign

            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;

            for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;



            if (pos >= 0)
            {

                if (bi1.data[pos] > bi2.data[pos])

                    return true;

                return false;

            }

            return false;

        }





        public static bool operator <(BigInteger bi1, BigInteger bi2)
        {

            int pos = maxLength - 1;



            // bi1 is negative, bi2 is positive

            if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)

                return true;



                // bi1 is positive, bi2 is negative

            else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)

                return false;



            // same sign

            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;

            for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;



            if (pos >= 0)
            {

                if (bi1.data[pos] < bi2.data[pos])

                    return true;

                return false;

            }

            return false;

        }





        public static bool operator >=(BigInteger bi1, BigInteger bi2)
        {

            return (bi1 == bi2 || bi1 > bi2);

        }





        public static bool operator <=(BigInteger bi1, BigInteger bi2)
        {

            return (bi1 == bi2 || bi1 < bi2);

        }





        //***********************************************************************

        // Private function that supports the division of two numbers with

        // a divisor that has more than 1 digit.

        //

        // Algorithm taken from [1]

        //***********************************************************************



        private static void multiByteDivide(BigInteger bi1, BigInteger bi2,

                                            BigInteger outQuotient, BigInteger outRemainder)
        {

            uint[] result = new uint[maxLength];



            int remainderLen = bi1.dataLength + 1;

            uint[] remainder = new uint[remainderLen];



            uint mask = 0x80000000;

            uint val = bi2.data[bi2.dataLength - 1];

            int shift = 0, resultPos = 0;



            while (mask != 0 && (val & mask) == 0)
            {

                shift++; mask >>= 1;

            }



            //Console.WriteLine("shift = {0}", shift);

            //Console.WriteLine("Before bi1 Len = {0}, bi2 Len = {1}", bi1.dataLength, bi2.dataLength);



            for (int i = 0; i < bi1.dataLength; i++)

                remainder[i] = bi1.data[i];

            shiftLeft(remainder, shift);

            bi2 = bi2 << shift;



            /*

            Console.WriteLine("bi1 Len = {0}, bi2 Len = {1}", bi1.dataLength, bi2.dataLength);

            Console.WriteLine("dividend = " + bi1 + "\ndivisor = " + bi2);

            for(int q = remainderLen - 1; q >= 0; q--)

                    Console.Write("{0:x2}", remainder[q]);

            Console.WriteLine();

            */



            int j = remainderLen - bi2.dataLength;

            int pos = remainderLen - 1;



            ulong firstDivisorByte = bi2.data[bi2.dataLength - 1];

            ulong secondDivisorByte = bi2.data[bi2.dataLength - 2];



            int divisorLen = bi2.dataLength + 1;

            uint[] dividendPart = new uint[divisorLen];



            while (j > 0)
            {

                ulong dividend = ((ulong)remainder[pos] << 32) + (ulong)remainder[pos - 1];

                //Console.WriteLine("dividend = {0}", dividend);



                ulong q_hat = dividend / firstDivisorByte;

                ulong r_hat = dividend % firstDivisorByte;



                //Console.WriteLine("q_hat = {0:X}, r_hat = {1:X}", q_hat, r_hat);



                bool done = false;

                while (!done)
                {

                    done = true;



                    if (q_hat == 0x100000000 ||

                       (q_hat * secondDivisorByte) > ((r_hat << 32) + remainder[pos - 2]))
                    {

                        q_hat--;

                        r_hat += firstDivisorByte;



                        if (r_hat < 0x100000000)

                            done = false;

                    }

                }



                for (int h = 0; h < divisorLen; h++)

                    dividendPart[h] = remainder[pos - h];



                BigInteger kk = new BigInteger(dividendPart);

                BigInteger ss = bi2 * (long)q_hat;



                //Console.WriteLine("ss before = " + ss);

                while (ss > kk)
                {

                    q_hat--;

                    ss -= bi2;

                    //Console.WriteLine(ss);

                }

                BigInteger yy = kk - ss;



                //Console.WriteLine("ss = " + ss);

                //Console.WriteLine("kk = " + kk);

                //Console.WriteLine("yy = " + yy);



                for (int h = 0; h < divisorLen; h++)

                    remainder[pos - h] = yy.data[bi2.dataLength - h];



                /*

                Console.WriteLine("dividend = ");

                for(int q = remainderLen - 1; q >= 0; q--)

                        Console.Write("{0:x2}", remainder[q]);

                Console.WriteLine("\n************ q_hat = {0:X}\n", q_hat);

                */



                result[resultPos++] = (uint)q_hat;



                pos--;

                j--;

            }



            outQuotient.dataLength = resultPos;

            int y = 0;

            for (int x = outQuotient.dataLength - 1; x >= 0; x--, y++)

                outQuotient.data[y] = result[x];

            for (; y < maxLength; y++)

                outQuotient.data[y] = 0;



            while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)

                outQuotient.dataLength--;



            if (outQuotient.dataLength == 0)

                outQuotient.dataLength = 1;



            outRemainder.dataLength = shiftRight(remainder, shift);



            for (y = 0; y < outRemainder.dataLength; y++)

                outRemainder.data[y] = remainder[y];

            for (; y < maxLength; y++)

                outRemainder.data[y] = 0;

        }





        //***********************************************************************

        // Private function that supports the division of two numbers with

        // a divisor that has only 1 digit.

        //***********************************************************************



        private static void singleByteDivide(BigInteger bi1, BigInteger bi2,

                                             BigInteger outQuotient, BigInteger outRemainder)
        {

            uint[] result = new uint[maxLength];

            int resultPos = 0;



            // copy dividend to reminder

            for (int i = 0; i < maxLength; i++)

                outRemainder.data[i] = bi1.data[i];

            outRemainder.dataLength = bi1.dataLength;



            while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)

                outRemainder.dataLength--;



            ulong divisor = (ulong)bi2.data[0];

            int pos = outRemainder.dataLength - 1;

            ulong dividend = (ulong)outRemainder.data[pos];



            //Console.WriteLine("divisor = " + divisor + " dividend = " + dividend);

            //Console.WriteLine("divisor = " + bi2 + "\ndividend = " + bi1);



            if (dividend >= divisor)
            {

                ulong quotient = dividend / divisor;

                result[resultPos++] = (uint)quotient;



                outRemainder.data[pos] = (uint)(dividend % divisor);

            }

            pos--;



            while (pos >= 0)
            {

                //Console.WriteLine(pos);



                dividend = ((ulong)outRemainder.data[pos + 1] << 32) + (ulong)outRemainder.data[pos];

                ulong quotient = dividend / divisor;

                result[resultPos++] = (uint)quotient;



                outRemainder.data[pos + 1] = 0;

                outRemainder.data[pos--] = (uint)(dividend % divisor);

                //Console.WriteLine(">>>> " + bi1);

            }



            outQuotient.dataLength = resultPos;

            int j = 0;

            for (int i = outQuotient.dataLength - 1; i >= 0; i--, j++)

                outQuotient.data[j] = result[i];

            for (; j < maxLength; j++)

                outQuotient.data[j] = 0;



            while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)

                outQuotient.dataLength--;



            if (outQuotient.dataLength == 0)

                outQuotient.dataLength = 1;



            while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)

                outRemainder.dataLength--;

        }





        //***********************************************************************

        // Overloading of division operator

        //***********************************************************************



        public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
        {

            BigInteger quotient = new BigInteger();

            BigInteger remainder = new BigInteger();



            int lastPos = maxLength - 1;

            bool divisorNeg = false, dividendNeg = false;



            if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
            {

                bi1 = -bi1;

                dividendNeg = true;

            }

            if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative
            {

                bi2 = -bi2;

                divisorNeg = true;

            }



            if (bi1 < bi2)
            {

                return quotient;

            }



            else
            {

                if (bi2.dataLength == 1)

                    singleByteDivide(bi1, bi2, quotient, remainder);

                else

                    multiByteDivide(bi1, bi2, quotient, remainder);



                if (dividendNeg != divisorNeg)

                    return -quotient;



                return quotient;

            }

        }





        //***********************************************************************

        // Overloading of modulus operator

        //***********************************************************************



        public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
        {

            BigInteger quotient = new BigInteger();

            BigInteger remainder = new BigInteger(bi1);



            int lastPos = maxLength - 1;

            bool dividendNeg = false;



            if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
            {

                bi1 = -bi1;

                dividendNeg = true;

            }

            if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative

                bi2 = -bi2;



            if (bi1 < bi2)
            {

                return remainder;

            }



            else
            {

                if (bi2.dataLength == 1)

                    singleByteDivide(bi1, bi2, quotient, remainder);

                else

                    multiByteDivide(bi1, bi2, quotient, remainder);



                if (dividendNeg)

                    return -remainder;



                return remainder;

            }

        }





        //***********************************************************************

        // Overloading of bitwise AND operator

        //***********************************************************************



        public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
        {

            BigInteger result = new BigInteger();



            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;



            for (int i = 0; i < len; i++)
            {

                uint sum = (uint)(bi1.data[i] & bi2.data[i]);

                result.data[i] = sum;

            }



            result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            return result;

        }





        //***********************************************************************

        // Overloading of bitwise OR operator

        //***********************************************************************



        public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
        {

            BigInteger result = new BigInteger();



            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;



            for (int i = 0; i < len; i++)
            {

                uint sum = (uint)(bi1.data[i] | bi2.data[i]);

                result.data[i] = sum;

            }



            result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            return result;

        }





        //***********************************************************************

        // Overloading of bitwise XOR operator

        //***********************************************************************



        public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
        {

            BigInteger result = new BigInteger();



            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;



            for (int i = 0; i < len; i++)
            {

                uint sum = (uint)(bi1.data[i] ^ bi2.data[i]);

                result.data[i] = sum;

            }



            result.dataLength = maxLength;



            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)

                result.dataLength--;



            return result;

        }





        //***********************************************************************

        // Returns max(this, bi)

        //***********************************************************************



        public BigInteger max(BigInteger bi)
        {

            if (this > bi)

                return (new BigInteger(this));

            else

                return (new BigInteger(bi));

        }





        //***********************************************************************

        // Returns min(this, bi)

        //***********************************************************************



        public BigInteger min(BigInteger bi)
        {

            if (this < bi)

                return (new BigInteger(this));

            else

                return (new BigInteger(bi));



        }





        //***********************************************************************

        // Returns the absolute value

        //***********************************************************************



        public BigInteger abs()
        {

            if ((this.data[maxLength - 1] & 0x80000000) != 0)

                return (-this);

            else

                return (new BigInteger(this));

        }





        //***********************************************************************

        // Returns a string representing the BigInteger in base 10.

        //***********************************************************************



        public override string ToString()
        {

            return ToString(10);

        }





        //***********************************************************************

        // Returns a string representing the BigInteger in sign-and-magnitude

        // format in the specified radix.

        //

        // Example

        // -------

        // If the value of BigInteger is -255 in base 10, then

        // ToString(16) returns "-FF"

        //

        //***********************************************************************



        public string ToString(int radix)
        {

            if (radix < 2 || radix > 36)

                throw (new ArgumentException("Radix must be >= 2 and <= 36"));



            string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string result = "";



            BigInteger a = this;



            bool negative = false;

            if ((a.data[maxLength - 1] & 0x80000000) != 0)
            {

                negative = true;

                try
                {

                    a = -a;

                }

                catch (Exception) { }

            }



            BigInteger quotient = new BigInteger();

            BigInteger remainder = new BigInteger();

            BigInteger biRadix = new BigInteger(radix);



            if (a.dataLength == 1 && a.data[0] == 0)

                result = "0";

            else
            {

                while (a.dataLength > 1 || (a.dataLength == 1 && a.data[0] != 0))
                {

                    singleByteDivide(a, biRadix, quotient, remainder);



                    if (remainder.data[0] < 10)

                        result = remainder.data[0] + result;

                    else

                        result = charSet[(int)remainder.data[0] - 10] + result;



                    a = quotient;

                }

                if (negative)

                    result = "-" + result;

            }



            return result;

        }





        //***********************************************************************

        // Returns a hex string showing the contains of the BigInteger

        //

        // Examples

        // -------

        // 1) If the value of BigInteger is 255 in base 10, then

        //    ToHexString() returns "FF"

        //

        // 2) If the value of BigInteger is -255 in base 10, then

        //    ToHexString() returns ".....FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF01",

        //    which is the 2's complement representation of -255.

        //

        //***********************************************************************



        public string ToHexString()
        {

            string result = data[dataLength - 1].ToString("X");



            for (int i = dataLength - 2; i >= 0; i--)
            {

                result += data[i].ToString("X8");

            }



            return result;

        }







        //***********************************************************************

        // Modulo Exponentiation

        //***********************************************************************



 







        //***********************************************************************

        // Fast calculation of modular reduction using Barrett's reduction.

        // Requires x < b^(2k), where b is the bas
    }
}