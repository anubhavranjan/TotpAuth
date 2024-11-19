namespace TotpAuth.Test
{
    [TestClass]
    public class TotpAuthUnitTest
    {
        static string key = string.Empty;
        static string otp = string.Empty;
        [TestMethod]
        public void GenerateKey()
        {
            key = Authenticator.GenerateKey();
            Assert.IsTrue(!string.IsNullOrEmpty(key));
        }

        [TestMethod]
        public void GenerateOtp()
        {
            var totp = new TotpAuth.Authenticator();
            otp =  totp.GetCode(key);
            Assert.IsTrue(!string.IsNullOrEmpty(otp));
        }

        [TestMethod]
        public void Validate()
        {
            var totp = new TotpAuth.Authenticator();
            var valid = totp.CheckCode(key, otp);
            Assert.IsTrue(valid);
        }
    }
}