using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace wServer.networking
{
    public class RSA
    {
/*        public static readonly RSA Instance = new RSA(@"
-----BEGIN RSA PRIVATE KEY-----
MIICXAIBAAKBgQCNvHpbCxcoSB0wjttR2gnkZbvHcXa2wbftmHRoYIfbChoc8R12
e0KtVz6AcbExP8PbayOftnl1bcI2lAGz66OqCnMygfhIW1aTg7QxfibhtkZLcs0e
7hAER1xeCMaOho/zyLIQvZ+qHUcdBhAwVDKP7bZ281b3OUpOBG5Hxt5bPwIDAQAB
AoGBAIOQHT8fR0qTzcyB/mC29JG2QRx7XMd9f54i8oLkf5a5hM2ynjeZaKYAIrsV
TXW6i7HDfJjGx21SCYGh1wbMRuimBydnYMax/JgtnrL69U98dqExPRgjYiZieZla
DumuuP+cZM2fpunY8ndDoVWNyGPj6tXpP+BD6IFrQLEBZQRpAkEA9GsM7Jt+pTok
TuPPcOxzFca6oPthfX00OIEtbmoN89c+7jyIvNlzsXGETRN9mdisQ9KPy/QN3Kob
Eg5Z6q1a9QJBAJRz1Gd6rte9++H6vq9YaOeZq76XM6bGQVhyIKr1ILtg8TGVtiyH
gOL8NvgMnxIcLC/rceDRurBpsDKc8pPDZOMCQAP7qp5AenPe2rCebcb9U3LLZkcx
UYll/O/eywq9l7SdkVz4h5HsSUJfAzTuWGGlckk4qTc9puwtqXtF2JlGcfECQAuv
no3Sy4ayLuzYF0CoXgG1SB7FukwrmSNEQKwUhdIaTIJRvbh9pji4D/+wxqjfTN8s
0pcXC3Itr7AcSMA3Bm8CQHnYYIa06MiCs7AH44Lic34jcbn++Y2a4Mz1nU14KFsR
YPNM0YKNTIRRnIEnBOG7Eyh0eXr5PtG796eWV+58UkQ=
-----END RSA PRIVATE KEY-----");*/ // old

        public static readonly RSA Instance = new RSA(@"
-----BEGIN RSA PRIVATE KEY-----
MIIBOAIBAAJAeyjMOLhcK4o2AnFRhn8vPteUy5Fux/cXN/J+wT/zYIEUINo02frn
+Kyxx0RIXJ3CvaHkwmueVL8ytfqo8Ol/OwIDAQABAkAmJBRa3H1u3narevyMcobn
J0xlXry4IMWIBglLP8rXb7FXSnvma6juXyu10mZ1+jSnexQkmhxaKGgaJhjWt6xB
AiEA3gaD5P1j8ERRJ4BN9eeGIN9hrV57pyaZAFWyO9a6SycCIQCOAV1vosaaishk
X88dx4ZLKwtLrTeytjZQNlQX+jDHzQIgFmx8B7Wb0VllBONNfGd8wXcuK09el7wr
OcBt9uMx/4MCIB3Fk31QNys3ZYQFwjqQFku0Ho4jJsZFBWYTvdW5EnkRAiAmt4WB
+cEKJsAP8kkScGt0CLGURc6CMRnEB7IF0zX0iA==
-----END RSA PRIVATE KEY-----");

        RsaEngine engine;
        AsymmetricKeyParameter key;
        private RSA(string privPem)
        {
            key = (new PemReader(new StringReader(privPem.Trim())).ReadObject() as AsymmetricCipherKeyPair).Private;
            engine = new RsaEngine();
            engine.Init(true, key);
        }

        public string Decrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            byte[] dat = Convert.FromBase64String(str);
            var encoding = new Pkcs1Encoding(engine);
            encoding.Init(false, key);
            return Encoding.UTF8.GetString(encoding.ProcessBlock(dat, 0, dat.Length));
        }
        public string Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            byte[] dat = Encoding.UTF8.GetBytes(str);
            var encoding = new Pkcs1Encoding(engine);
            encoding.Init(true, key);
            return Convert.ToBase64String(encoding.ProcessBlock(dat, 0, dat.Length));
        }
    }
}
