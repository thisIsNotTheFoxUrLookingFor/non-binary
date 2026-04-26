namespace NonBinary.WindowsAgent.Models;

public class RemotePolicy
{
    public string PolicyName { get; set; } = "NonBinary-Supplemental";
    public List<SignerRule> Signers { get; set; } = new();
    public List<PathRule> Paths { get; set; } = new();
    public List<HashRule> Hashes { get; set; } = new();
    public string Signature { get; set; } = ""; // HMAC-SHA256 of the payload
}

public class SignerRule { public string PublisherName { get; set; } = ""; public string ProductName { get; set; } = ""; public string BinaryName { get; set; } = ""; }
public class PathRule { public string Path { get; set; } = ""; public bool Recursive { get; set; } = true; }
public class HashRule { public string FileName { get; set; } = ""; public string Sha256 { get; set; } = ""; }