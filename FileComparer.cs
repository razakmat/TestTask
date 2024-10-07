using System.Numerics;

namespace TestTask
{
    /// <summary>
    /// Class <c>FileComparer</c> compares two files, whether they have same contents.
    /// Comparison is done by comparing bytes in contents of files.
    /// </summary>
    public class FileComparer
    {
        /// <summary>
        /// Attribut <c>_buffer_size</c> gives number of bytes, that are read at once from file.
        /// Current size is set to 1MB bytes.
        /// </summary>
        private static readonly int _buffer_size = 1048576;

        /// <summary>
        /// Static method <c>Compare</c> compares two given files.
        /// </summary>
        /// <param name="source"> First file(Source), type of FileInfo</param>
        /// <param name="replica"> Second file(Replica), type of FileInfo</param>
        /// <returns>Returns true if files have identical contents, else returns false</returns>
        static public bool Compare(FileInfo source, FileInfo replica)
        {
            // first checks, if size is same for both files
            // if size is different, files are not identical and method returns false
            if (!SameSize(source,replica))
                return false;
            
            // then checks contents
            if (!SameContent(source,replica)) 
                return false;

            return true;
        }

        /// <summary>
        /// Static method <c>SameSize</c> checks if two given files have same size in bytes.
        /// </summary>
        /// <param name="source">First file(Source), type of FileInfo</param></param>
        /// <param name="replica">Second file(Replica), type of FileInfo</param></param>
        /// <returns>Return true if size is the same, else returns false</returns>
        static private bool SameSize(FileInfo source, FileInfo replica)
        {
            return source.Length == replica.Length;
        }

        /// <summary>
        /// Static method <c>CheckBuffer</c> checks if bytes in two buffers are same.
        /// Uses SIMD instructions (if hardware supports it) for better effectivity.
        /// </summary>
        /// <param name="buffer_source">buffer holding bytes from source file</param>
        /// <param name="buffer_replica">buffer holding bytes from replica file</param>
        /// <param name="size">Number of bytes in buffers for comparison</param>
        /// <returns>Returns true if bytes in buffers are the same, else returns false</returns>
        static private bool CheckBuffer(byte[] buffer_source, byte[] buffer_replica, int size)
        {
            int check = 0;
            while (check < size)
            {
                if (!Vector.EqualsAll(new Vector<byte>(buffer_source, check), new Vector<byte>(buffer_replica, check)))
                        return false;
                check += Vector<byte>.Count;
            }

            return true;
        }

        /// <summary>
        /// Static method <c>SameContent</c> checks if two files have the same contents.
        /// </summary>
        /// <param name="source"> First file(Source), type of FileInfo</param>
        /// <param name="replica"> Second file(Replica), type of FileInfo</param>
        /// <returns>Returns true if contents(bytes) of both files are the same, else returns false</returns>
        static private bool SameContent(FileInfo source, FileInfo replica)
        {
            // creates streams for both files
            using FileStream replica_stream = replica.Open(FileMode.Open, FileAccess.ReadWrite);
            using FileStream source_stream = source.Open(FileMode.Open, FileAccess.Read);

            // creates buffers for <c>_buffer_size</c> bytes for both files
            byte[] buffer_source = new byte[_buffer_size];
            byte[] buffer_replica = new byte[_buffer_size];


            // while cyklus till all bytes are read or difference is found
            while (true)
            {
                // fill buffers from streams
                // returns (read_s,read_r) number of read bytes
                int read_s = source_stream.Read(buffer_source, 0, _buffer_size);
                int read_r = replica_stream.Read(buffer_replica, 0, _buffer_size);

                // should not happen
                if (read_r != read_s)
                    return false;

                // if nothing was read from streams, both contents are the same, returns true
                if (read_s == 0)
                    return true;

                // compares bytes in both buffers
                // if difference, returns false
                if (!CheckBuffer(buffer_source,buffer_replica,read_r))
                    return false;
            }
        }
    }
}