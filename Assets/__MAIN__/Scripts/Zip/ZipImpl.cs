using System;
using System.IO;
using COL.UnityGameWheels.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace COL.UnityGameWheels.Demo
{
    public class ZipImpl : IZipImpl
    {
        private const int CachedBytesLength = 0x1000;

        [ThreadStatic]
        private static byte[] s_CachedBytes = null;

        private static byte[] CachedBytes => s_CachedBytes ?? (s_CachedBytes = new byte[CachedBytesLength]);

        public void Unzip(Stream archiveStream, Stream dstStream)
        {
            GZipInputStream gZipInputStream = new GZipInputStream(archiveStream);
            int bytesRead = 0;
            while ((bytesRead = gZipInputStream.Read(CachedBytes, 0, CachedBytesLength)) > 0)
            {
                dstStream.Write(CachedBytes, 0, bytesRead);
            }
        }

        public void Zip(Stream srcStream, Stream archiveStream)
        {
            GZipOutputStream gZipOutputStream = new GZipOutputStream(archiveStream);
            int bytesRead = 0;
            while ((bytesRead = srcStream.Read(CachedBytes, 0, CachedBytesLength)) > 0)
            {
                gZipOutputStream.Write(CachedBytes, 0, bytesRead);
            }

            gZipOutputStream.Finish();
            ProcessHeader(archiveStream);
        }

        private static void ProcessHeader(Stream archiveStream)
        {
            if (archiveStream.Length < 8L) return;
            var current = archiveStream.Position;
            archiveStream.Position = 4L;
            archiveStream.WriteByte(25);
            archiveStream.WriteByte(134);
            archiveStream.WriteByte(2);
            archiveStream.WriteByte(32);
            archiveStream.Position = current;
        }
    }
}