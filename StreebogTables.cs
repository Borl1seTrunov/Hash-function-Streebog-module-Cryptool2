using System;

namespace Streebog
{
    internal static class StreebogTables
    {
        internal static readonly ulong[][] C = new ulong[12][];

        internal static readonly byte[] Pi = new byte[]
        {
            252, 238, 221, 17, 207, 110, 49, 22, 251, 196, 250, 218, 35, 197, 4, 77,
            233, 119, 240, 219, 147, 46, 153, 186, 23, 54, 241, 187, 20, 205, 95, 193,
            249, 24, 101, 90, 226, 92, 239, 33, 129, 28, 60, 66, 139, 1, 142, 79,
            5, 132, 2, 174, 227, 106, 143, 160, 6, 11, 237, 152, 127, 212, 211, 31,
            235, 52, 44, 81, 234, 200, 72, 171, 242, 42, 104, 162, 253, 58, 206, 204,
            181, 112, 14, 86, 8, 12, 118, 18, 191, 114, 19, 71, 156, 183, 93, 135,
            21, 161, 150, 41, 16, 123, 154, 199, 243, 145, 120, 111, 157, 158, 178, 177,
            50, 117, 25, 61, 255, 53, 138, 126, 109, 84, 198, 128, 195, 189, 13, 87,
            223, 245, 36, 169, 62, 168, 67, 201, 215, 121, 214, 246, 124, 34, 185, 3,
            224, 15, 236, 222, 122, 148, 176, 188, 220, 232, 40, 80, 78, 51, 10, 74,
            167, 151, 96, 115, 30, 0, 98, 68, 26, 184, 56, 130, 100, 159, 38, 65,
            173, 69, 70, 146, 39, 94, 85, 47, 140, 163, 165, 125, 105, 213, 149, 59,
            7, 88, 179, 64, 134, 172, 29, 247, 48, 55, 107, 228, 136, 217, 231, 137,
            225, 27, 131, 73, 76, 63, 248, 254, 141, 83, 170, 144, 202, 216, 133, 97,
            32, 113, 103, 164, 45, 43, 9, 91, 203, 155, 37, 208, 190, 229, 108, 82,
            89, 166, 116, 210, 230, 244, 180, 192, 209, 102, 175, 194, 57, 75, 99, 182
        };

        internal static readonly byte[] Tau = new byte[]
        {
            0, 8, 16, 24, 32, 40, 48, 56, 1, 9, 17, 25, 33, 41, 49, 57,
            2, 10, 18, 26, 34, 42, 50, 58, 3, 11, 19, 27, 35, 43, 51, 59,
            4, 12, 20, 28, 36, 44, 52, 60, 5, 13, 21, 29, 37, 45, 53, 61,
            6, 14, 22, 30, 38, 46, 54, 62, 7, 15, 23, 31, 39, 47, 55, 63
        };

        internal static readonly ulong[] A = new ulong[]
        {
            0x8e20faa72ba0b470UL, 0x47107ddd9b505a38UL,
            0xad08b0e0c3282d1cUL, 0xd8045870ef14980eUL,
            0x6c022c38f90a4c07UL, 0x3601161cf205268dUL,
            0x1b8e0b0e798c13c8UL, 0x83478b07b2468764UL,
            0xa011d380818e8f40UL, 0x5086e740ce47c920UL,
            0x2843fd2067adea10UL, 0x14aff010bdd87508UL,
            0x0ad97808d06cb404UL, 0x05e23c0468365a02UL,
            0x8c711e02341b2d01UL, 0x46b60f011a83988eUL,
            0x90dab52a387ae76fUL, 0x486dd4151c3dfdb9UL,
            0x24b86a840e90f0d2UL, 0x125c354207487869UL,
            0x092e94218d243cbaUL, 0x8a174a9ec8121e5dUL,
            0x4585254f64090fa0UL, 0xaccc9ca9328a8950UL,
            0x9d4df05d5f661451UL, 0xc0a878a0a1330aa6UL,
            0x60543c50de970553UL, 0x302a1e286fc58ca7UL,
            0x18150f14b9ec46ddUL, 0x0c84890ad27623e0UL,
            0x0642ca05693b9f70UL, 0x0321658cba93c138UL,
            0x86275df09ce8aaa8UL, 0x439da0784e745554UL,
            0xafc0503c273aa42aUL, 0xd960281e9d1d5215UL,
            0xe230140fc0802984UL, 0x71180a8960409a42UL,
            0xb60c05ca30204d21UL, 0x5b068c651810a89eUL,
            0x456c34887a3805b9UL, 0xac361a443d1c8cd2UL,
            0x561b0d22900e4669UL, 0x2b838811480723baUL,
            0x9bcf4486248d9f5dUL, 0xc3e9224312c8c1a0UL,
            0xeffa11af0964ee50UL, 0xf97d86d98a327728UL,
            0xe4fa2054a80b329cUL, 0x727d102a548b194eUL,
            0x39b008152acb8227UL, 0x9258048415eb419dUL,
            0x492c024284fbaec0UL, 0xaa16012142f35760UL,
            0x550b8e9e21f7a530UL, 0xa48b474f9ef5dc18UL,
            0x70a6a56e2440598eUL, 0x3853dc371220a247UL,
            0x1ca76e95091051adUL, 0x0edd37c48a08a6d8UL,
            0x07e095624504536cUL, 0x8d70c431ac02a736UL,
            0xc83862965601dd1bUL, 0x641c314b2b8ee083UL
        };

        static StreebogTables()
        {

            C[0] = new ulong[] {
                0xdd806559f2a64507UL,
                0x05767436cc744d23UL,
                0xa2422a08a460d315UL,
                0x4b7ce09192676901UL,
                0x714eb88d7585c4fcUL,
                0x2f6a76432e45d016UL,
                0xebcb2f81c0657c1fUL,
                0xb1085bda1ecadae9UL
            };
            C[1] = new ulong[] {
                0xe679047021b19bb7UL,
                0x55dda21bd7cbcd56UL,
                0x5cb561c2db0aa7caUL,
                0x9ab5176b12d69958UL,
                0x61d55e0f16b50131UL,
                0xf3feea720a232b98UL,
                0x4fe39d460f70b5d7UL,
                0x6fa3b58aa99d2f1aUL
            };
            C[2] = new ulong[] {
                0x991e96f50aba0ab2UL,
                0xc2b6f443867adb31UL,
                0xc1c93a376062db09UL,
                0xd3e20fe490359eb1UL,
                0xf2ea7514b1297b7bUL,
                0x06f15e5f529c1f8bUL,
                0x0a39fc286a3d8435UL,
                0xf574dcac2bce2fc7UL
            };
            C[3] = new ulong[] {
                0x220cbebc84e3d12eUL,
                0x3453eaa193e837f1UL,
                0xd8b71333935203beUL,
                0xa9d72c82ed03d675UL,
                0x9d721cad685e353fUL,
                0x488e857e335c3c7dUL,
                0xf948e1a05d71e4ddUL,
                0xef1fdfb3e81566d2UL
            };
            C[4] = new ulong[] {
                0x601758fd7c6cfe57UL,
                0x7a56a27ea9ea63f5UL,
                0xdfff00b723271a16UL,
                0xbfcd1747253af5a3UL,
                0x359e35d7800fffbdUL,
                0x7f151c1f1686104aUL,
                0x9a3f410c6ca92363UL,
                0x4bea6bacad474799UL
            };
            C[5] = new ulong[] {
                0xfa68407a46647d6eUL,
                0xbf71c57236904f35UL,
                0x0af21f66c2bec6b6UL,
                0xcffaa6b71c9ab7b4UL,
                0x187f9ab49af08ec6UL,
                0x2d66c4f95142a46cUL,
                0x6fa4c33b7a3039c0UL,
                0xae4faeae1d3ad3d9UL
            };
            C[6] = new ulong[] {
                0x8886564d3a14d493UL,
                0x3517454ca23c4af3UL,
                0x06476983284a0504UL,
                0x0992abc52d822c37UL,
                0xd3473e33197a93c9UL,
                0x399ec6c7e6bf87c9UL,
                0x51ac86febf240954UL,
                0xf4c70e16eeaac5ecUL
            };
            C[7] = new ulong[] {
                0xa47f0dd4bf02e71eUL,
                0x36acc2355951a8d9UL,
                0x69d18d2bd1a5c42fUL,
                0xf4892bcb929b0690UL,
                0x89b4443b4ddbc49aUL,
                0x4eb7f8719c36de1eUL,
                0x03e7aa020c6e4141UL,
                0x9b1f5b424d93c9a7UL
            };
            C[8] = new ulong[] {
                0x7261445183235adbUL,
                0x0e38dc92cb1f2a60UL,
                0x7b2b8a9aa6079c54UL,
                0x800a440bdbb2ceb1UL,
                0x3cd955b7e00d0984UL,
                0x3a7d3a1b25894224UL,
                0x944c9ad8ec165fdeUL,
                0x378f5a541631229bUL
            };
            C[9] = new ulong[] {
                0x74b4c7fb98459cedUL,
                0x3698fad1153bb6c3UL,
                0x7a1e6c303b7652f4UL,
                0x9fe76702af69334bUL,
                0x1fffe18a1b336103UL,
                0x8941e71cff8a78dbUL,
                0x382ae548b2e4f3f3UL,
                0xabbedea680056f52UL
            };
            C[10] = new ulong[] {
                0x6bcaa4cd81f32d1bUL,
                0xdea2594ac06fd85dUL,
                0xefbacd1d7d476e98UL,
                0x8a1d71efea48b9caUL,
                0x2001802114846679UL,
                0xd8fa6bbbebab0761UL,
                0x3002c6cd635afe94UL,
                0x7bcd9ed0efc889fbUL
            };
            C[11] = new ulong[] {
                0x48bc924af11bd720UL,
                0xfaf417d5d9b21b99UL,
                0xe71da4aa88e12852UL,
                0x5d80ef9d1891cc86UL,
                0xf82012d430219f9bUL,
                0xcda43c32bcdf1d77UL,
                0xd21380b00449b17aUL,
                0x378ee767f11631baUL
            };
        }
    }
}
