using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RugTech1
{
    public static class StructHelper
    {
        /// <summary>
        /// Reads in a block from a file and converts it to the struct
        /// type specified by the template parameter
        /// </summary>
        /// <typeparam name="T">type of the struct to read</typeparam>
        /// <param name="reader">reader</param>
        /// <returns>a instance of the struct T cast from the data in the reader</returns>
        public static T ReadStructure<T>(BinaryReader reader)
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            
            handle.Free();

            return theStructure;
        }

        public static void WriteStructure(BinaryWriter write, object obj)
        {
            int len = Marshal.SizeOf(obj);

            byte[] buffer = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, buffer, 0, len);

            Marshal.FreeHGlobal(ptr);

            write.Write(buffer); 
        }
    }
}
