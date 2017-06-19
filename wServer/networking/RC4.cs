using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace wServer.networking
{
    public class RC4
    {
        RC4Engine rc4;

        public RC4(byte[] key)
        {
            rc4 = new RC4Engine();
            rc4.Init(true, new KeyParameter(key));
        }

        public void Crypt(byte[] buf, int offset, int len)
        {
            rc4.ProcessBytes(buf, offset, len, buf, offset);
        }
    }
}
