using System.Security.Cryptography;
using System.Text;
using System.Xml;

public static class CloudfrontSignedURLUtils
{
    public static string ConvertPemToXML(string pemString)
    {
        var privateKey = pemString;
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey.ToCharArray());
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(rsa.ToXmlString(true));
        return xmlDoc.OuterXml;
    }

    public static string CreateCannedPrivateURL(string urlString, string durationUnits,
            string durationNumber, string policy, string privateKeyInXml, string privateKeyId)
    {

        TimeSpan timeSpanInterval = GetDuration(durationUnits, durationNumber);

        // Create the policy statement.
        string strPolicy = CreatePolicyStatement(policy,
            urlString, DateTime.Now, DateTime.Now.Add(timeSpanInterval));
        if ("Error!" == strPolicy) return "Invalid time frame.  Start time cannot be greater than end time.";

        // Copy the expiration time defined by policy statement.
        string strExpiration = CopyExpirationTimeFromPolicy(strPolicy);

        // Read the policy into a byte buffer.
        byte[] bufferPolicy = Encoding.ASCII.GetBytes(strPolicy);

        // Initialize the SHA1CryptoServiceProvider object and hash the policy data.
        using (var cryptoSHA1 = new SHA1CryptoServiceProvider())
        {
            bufferPolicy = cryptoSHA1.ComputeHash(bufferPolicy);

            // Initialize the RSACryptoServiceProvider object.
            RSACryptoServiceProvider providerRSA = new RSACryptoServiceProvider();
            providerRSA.FromXmlString(privateKeyInXml);
            RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(providerRSA);
            rsaFormatter.SetHashAlgorithm("SHA1");
            byte[] signedPolicyHash = rsaFormatter.CreateSignature(bufferPolicy);

            // Convert the signed policy to URL safe base 64 encoding.
            string strSignedPolicy = ToUrlSafeBase64String(signedPolicyHash);

            // Concatenate the URL, the timestamp, the signature, and the key pair ID to form the private URL.
            return urlString + "?Expires=" + strExpiration + "&Signature=" + strSignedPolicy + "&Key-Pair-Id=" + privateKeyId;
        }
    }

    public static string ToUrlSafeBase64String(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('=', '_')
            .Replace('/', '~');
    }

    public static TimeSpan GetDuration(string units, string numUnits)
    {
        TimeSpan timeSpanInterval = new TimeSpan();
        switch (units)
        {
            case "seconds":
                timeSpanInterval = new TimeSpan(0, 0, 0, int.Parse(numUnits));
                break;
            case "minutes":
                timeSpanInterval = new TimeSpan(0, 0, int.Parse(numUnits), 0);
                break;
            case "hours":
                timeSpanInterval = new TimeSpan(0, int.Parse(numUnits), 0, 0);
                break;
            case "days":
                timeSpanInterval = new TimeSpan(int.Parse(numUnits), 0, 0, 0);
                break;
            default:
                Console.WriteLine("Invalid time units; use seconds, minutes, hours, or days");
                break;
        }
        return timeSpanInterval;
    }

    public static string CreatePolicyStatement(string policyStatementAsStr, string resourceUrl,
                          DateTime startTime, DateTime endTime)
    {
        // Create the policy statement.

        TimeSpan startTimeSpanFromNow = (startTime - DateTime.Now);
        TimeSpan endTimeSpanFromNow = (endTime - DateTime.Now);
        TimeSpan intervalStart =
            (DateTime.UtcNow.Add(startTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan intervalEnd =
            (DateTime.UtcNow.Add(endTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        int startTimestamp = (int)intervalStart.TotalSeconds; // START_TIME
        int endTimestamp = (int)intervalEnd.TotalSeconds;  // END_TIME

        if (startTimestamp > endTimestamp)
            return "Error!";

        // Replace variables in the policy statement.
        policyStatementAsStr = policyStatementAsStr.Replace("RESOURCE", resourceUrl);
        policyStatementAsStr = policyStatementAsStr.Replace("EXPIRES", endTimestamp.ToString());
        return policyStatementAsStr;

    }

    public static string CopyExpirationTimeFromPolicy(string policyStatement)
    {
        int startExpiration = policyStatement.IndexOf("EpochTime");
        string strExpirationRough = policyStatement.Substring(startExpiration + "EpochTime".Length);
        char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        List<char> listDigits = new List<char>(digits);
        StringBuilder buildExpiration = new StringBuilder(20);
        foreach (char c in strExpirationRough)
        {
            if (listDigits.Contains(c))
                buildExpiration.Append(c);
        }
        return buildExpiration.ToString();
    }
}