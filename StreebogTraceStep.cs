using System;

namespace Streebog
{
    public class StreebogTraceStep
    {
        public string Phase { get; set; }
        public string Description { get; set; }
        public int BlockIndex { get; set; }
        public int BlockCount { get; set; }
        public int Round { get; set; }
        public int DigestSize { get; set; }
        public ulong[] Iv { get; set; }
        public ulong[] H { get; set; }
        public ulong[] N { get; set; }
        public ulong[] Sigma { get; set; }
        public ulong[] M { get; set; }
        public ulong[] Data { get; set; }
        public ulong[] Ki { get; set; }
        public string Operation { get; set; }
        public bool IsFinal { get; set; }
        public bool IsGEnd { get; set; }

        public static ulong[] Clone(ulong[] src)
        {
            if (src == null) return null;
            ulong[] dst = new ulong[src.Length];
            Array.Copy(src, dst, src.Length);
            return dst;
        }
    }
}
