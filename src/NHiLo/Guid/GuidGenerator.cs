using System;
using System.Collections;

namespace NHiLo.Guid
{
    /// <summary>
    /// Create sequential guids to be used as keys.
    /// </summary>
    /// <remarks>
    /// These are v1 guids with the same logic as SQL Server's NEWSEQUENTIALID function. 
    /// Be aware that these guids are generated with the MAC address of the computer, so this security flaw might be exploited.
    /// Be sure this isn't a problem for you.
    /// </remarks>
    public class GuidGenerator : IKeyGenerator<string>
    {
        public string GetKey()
        {
            byte[] guidArray = System.Guid.NewGuid().ToByteArray();

            DateTime baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string 
            TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new System.Guid(guidArray).ToString();
        }
    }
}
