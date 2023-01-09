namespace Pulumi.Gcp.Storage;

//
// Summary:
//     Prevents public access to a bucket. Acceptable values are "inherited" or "enforced".
//     If "inherited", the bucket uses [public access prevention](https://cloud.google.com/storage/docs/public-access-prevention).
//     only if the bucket is subject to the public access prevention organization policy
//     constraint. Defaults to "inherited".
public class PublicAccessPrevention
{
    public const string Enforced = "enforced";
    public const string Inherited = "inherited";
}
