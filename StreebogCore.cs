using System;

namespace Streebog
{
    internal sealed class StreebogCore
    {
        private ulong[] h = new ulong[8];
        private ulong[] iv = new ulong[8];
        private ulong[] n = new ulong[8];
        private ulong[] sigma = new ulong[8];
        private byte[] buffer = new byte[64];
        private int bufsize;
        private int digestSize;
        private int blockIndex;

        public Action<StreebogTraceStep> Trace { get; set; }
        public bool SkipTrace { get; set; }

        public int BlockCount { get; set; }

        private static readonly ulong[] buffer512 = new ulong[]
        {
            0x200UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL
        };
        private static readonly ulong[] zero512 = new ulong[]
        {
            0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL, 0x0UL
        };

        public StreebogCore()
        {
            BlockCount = -1;
        }

        public bool CustomIv { get; private set; }

        public void Init(int digestSize)
        {
            Init(digestSize, null);
        }

        public void Init(int digestSize, byte[] customIv)
        {
            if (customIv != null && customIv.Length != 64)
            {
                throw new ArgumentException("Вектор инициализации IV должен содержать ровно 64 байта (512 бит).", "customIv");
            }

            this.digestSize = digestSize;
            CustomIv = customIv != null;

            if (CustomIv)
            {
                ulong[] vector = ToUlong(customIv, 0);
                Array.Copy(vector, this.iv, 8);
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    this.iv[i] = digestSize == 256 ? 0x0101010101010101UL : 0x0UL;
                }
            }

            Array.Copy(this.iv, this.h, 8);
            this.n = new ulong[8];
            this.sigma = new ulong[8];
            this.bufsize = 0;
            this.blockIndex = 0;
            Emit("Инициализация", CustomIv
                ? "Начальное состояние (задан IV): h = IV, N = 0, Σ = 0"
                : "Начальное состояние (стандартный IV): h = IV, N = 0, Σ = 0", -1, null);
        }

        private void Emit(string phase, string description, int round, ulong[] m, ulong[] data, ulong[] ki, string op, bool gEnd = false)
        {
            if (Trace == null || SkipTrace) return;
            Trace(new StreebogTraceStep
            {
                Phase = phase,
                Description = description,
                BlockIndex = blockIndex,
                BlockCount = BlockCount,
                Round = round,
                DigestSize = digestSize,
                Iv = StreebogTraceStep.Clone(iv),
                H = StreebogTraceStep.Clone(h),
                N = StreebogTraceStep.Clone(n),
                Sigma = StreebogTraceStep.Clone(sigma),
                M = StreebogTraceStep.Clone(m),
                Data = StreebogTraceStep.Clone(data),
                Ki = StreebogTraceStep.Clone(ki),
                Operation = op,
                IsFinal = false,
                IsGEnd = gEnd
            });
        }

        private void Emit(string phase, string description, int round, ulong[] m)
        {
            Emit(phase, description, round, m, null, null, null);
        }

        public void Update(byte[] data, int offset, int count)
        {
            int len = count;
            int pos = offset;

            if (this.bufsize != 0)
            {
                int chunksize = 64 - this.bufsize;
                if (chunksize > len)
                {
                    chunksize = len;
                }

                Array.Copy(data, pos, this.buffer, this.bufsize, chunksize);
                this.bufsize += chunksize;
                len -= chunksize;
                pos += chunksize;

                if (this.bufsize == 64)
                {
                    Stage2(this.h, this.n, this.buffer, 0);
                    this.bufsize = 0;
                }
            }

            while (len > 63)
            {
                Stage2(this.h, this.n, data, pos);
                pos += 64;
                len -= 64;
            }

            if (len > 0)
            {
                Array.Copy(data, pos, this.buffer, 0, len);
                this.bufsize = len;
            }
        }

        public byte[] Final()
        {
            ulong[] lastLen = new ulong[8];
            lastLen[0] = (ulong)(this.bufsize << 3);
            int lastBits = this.bufsize << 3;

            Pad();
            ulong[] mPad = ToUlong(this.buffer, 0);
            Emit("Дополнение", "Дополнение последнего блока: m ‖ 0x01 ‖ 00…00 (длина = " + lastBits + " бит)", -1, mPad);

            g(this.h, this.n, mPad, "Финальный блок");

            Add512(this.n, lastLen, this.n);
            Add512(this.sigma, mPad, this.sigma);
            Emit("Финализация", "N := N + " + lastBits + " (длина последнего блока в битах);  Σ := Σ + m_pad", -1, mPad);

            Emit("Финализация", "Далее: h := g_0(h, N)", -1, StreebogTraceStep.Clone(this.n));
            g(this.h, zero512, this.n, "g0(N)");
            Emit("Финализация", "Далее: h := g_0(h, Σ)", -1, StreebogTraceStep.Clone(this.sigma));
            g(this.h, zero512, this.sigma, "g0(Σ)");

            int firstWord = this.digestSize == 256 ? 4 : 0;
            byte[] digest = new byte[(8 - firstWord) * 8];
            for (int i = firstWord; i < 8; i++)
            {
                int dest = (i - firstWord) * 8;
                for (int j = 0; j < 8; j++)
                {
                    digest[dest + j] = (byte)((this.h[i] >> (8 * j)) & 0xFFUL);
                }
            }

            if (Trace != null)
            {
                Trace(new StreebogTraceStep
                {
                    Phase = "Готово",
                    Description = "Хэш вычислен",
                    BlockIndex = blockIndex,
                    BlockCount = BlockCount,
                    Round = -1,
                    DigestSize = digestSize,
                    Iv = StreebogTraceStep.Clone(iv),
                    H = StreebogTraceStep.Clone(h),
                    N = StreebogTraceStep.Clone(n),
                    Sigma = StreebogTraceStep.Clone(sigma),
                    M = null,
                    IsFinal = true
                });
            }

            this.bufsize = 0;
            return digest;
        }

        private void Stage2(ulong[] h, ulong[] n, byte[] block, int offset)
        {
            ulong[] m = ToUlong(block, offset);
            Emit("Блок", "Сжатие блока " + (blockIndex + 1) + ": h := g_N(h, m)", -1, m);
            g(h, n, m, "Блок " + (blockIndex + 1));
            Add512(n, buffer512, n);
            Add512(sigma, m, sigma);
            blockIndex++;
            Emit("Блок", "После блока " + blockIndex + ": N := N + 512,  Σ := Σ + m", -1, m);
        }

        private void Pad()
        {
            if (this.bufsize > 63)
            {
                return;
            }

            Array.Clear(this.buffer, this.bufsize, this.buffer.Length - this.bufsize);
            this.buffer[this.bufsize] = 0x01;
        }

        private void g(ulong[] h, ulong[] n, ulong[] m, string label)
        {
            string tag = "g/" + label;
            ulong[] data = new ulong[8];
            ulong[] K = new ulong[8];

            XorTo(h, n, K);
            Emit(tag, label + ": K := h ⊕ N", 0, m, data, K, "K := h ⊕ N");
            LpsStep(tag, label + ": ", "K", K, 0, m, data, K);

            XorTo(K, m, data);
            Emit(tag, label + ": data := K ⊕ m", 0, m, data, K, "data := K ⊕ m");

            for (int r = 1; r <= 12; r++)
            {
                string prefix = label + ": раунд " + r + ", ";

                LpsStep(tag, prefix, "data", data, r, m, data, K);

                XorInPlace(K, StreebogTables.C[r - 1]);
                Emit(tag, prefix + "K := K ⊕ C[" + (r - 1) + "]", r, m, data, K, "K := K ⊕ C[" + (r - 1) + "]");
                LpsStep(tag, prefix, "K", K, r, m, data, K);

                XorInPlace(data, K);
                Emit(tag, prefix + "data := data ⊕ K", r, m, data, K, "data := data ⊕ K");
            }

            XorInPlace(data, h);
            Emit(tag, label + ": data := data ⊕ h", 12, m, data, K, "data := data ⊕ h");
            XorTo(data, m, h);
            Emit(tag, label + ": h := data ⊕ m (результат g)", 12, m, data, K, "h := data ⊕ m", true);
        }

        private void LpsStep(string tag, string prefix, string name, ulong[] target, int round, ulong[] m, ulong[] data, ulong[] ki)
        {
            STransform(target);
            Emit(tag, prefix + name + " := S(" + name + ")", round, m, data, ki, name + " := S(" + name + ")");
            PTransform(target);
            Emit(tag, prefix + name + " := P(" + name + ")", round, m, data, ki, name + " := P(" + name + ")");
            LTransform(target);
            Emit(tag, prefix + name + " := L(" + name + ")", round, m, data, ki, name + " := L(" + name + ")");
        }

        private static void STransform(ulong[] state)
        {
            for (int i = 0; i < 8; i++)
            {
                ulong w = state[i];
                ulong r = 0;
                for (int j = 0; j < 8; j++)
                {
                    int b = (int)((w >> (8 * j)) & 0xFFUL);
                    r |= ((ulong)StreebogTables.Pi[b]) << (8 * j);
                }
                state[i] = r;
            }
        }

        private static void PTransform(ulong[] state)
        {
            byte[] src = new byte[64];
            for (int i = 0; i < 8; i++)
            {
                ulong w = state[i];
                int off = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    src[off + j] = (byte)((w >> (8 * j)) & 0xFFUL);
                }
            }
            byte[] dst = new byte[64];
            for (int k = 0; k < 64; k++)
            {
                dst[k] = src[StreebogTables.Tau[k]];
            }
            for (int i = 0; i < 8; i++)
            {
                ulong w = 0;
                int off = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    w |= ((ulong)dst[off + j]) << (8 * j);
                }
                state[i] = w;
            }
        }

        private static ulong LWord(ulong word)
        {
            ulong res = 0;
            for (int k = 0; k < 64; k++)
            {
                if (((word >> k) & 1UL) != 0)
                {
                    res ^= StreebogTables.A[63 - k];
                }
            }
            return res;
        }

        private static void LTransform(ulong[] state)
        {
            for (int i = 0; i < 8; i++)
            {
                state[i] = LWord(state[i]);
            }
        }

        private static void XorTo(ulong[] x, ulong[] y, ulong[] result)
        {
            for (int i = 0; i < 8; i++)
            {
                result[i] = x[i] ^ y[i];
            }
        }

        private static void XorInPlace(ulong[] dst, ulong[] src)
        {
            for (int i = 0; i < 8; i++)
            {
                dst[i] ^= src[i];
            }
        }

        private static void Add512(ulong[] x, ulong[] y, ulong[] result)
        {
            ulong carry = 0;
            for (int i = 0; i < 8; i++)
            {
                ulong left = x[i];
                ulong right = y[i];
                ulong sum = unchecked(left + right);
                ulong newCarry = sum < left ? 1UL : 0UL;
                ulong sum2 = unchecked(sum + carry);
                if (sum2 < sum) newCarry = 1UL;
                result[i] = sum2;
                carry = newCarry;
            }
        }

        private static ulong[] ToUlong(byte[] data, int offset)
        {
            ulong[] result = new ulong[8];
            for (int i = 0; i < 8; i++)
            {
                ulong v = 0;
                for (int j = 7; j >= 0; j--)
                {
                    v = (v << 8) | data[offset + i * 8 + j];
                }
                result[i] = v;
            }
            return result;
        }
    }
}
