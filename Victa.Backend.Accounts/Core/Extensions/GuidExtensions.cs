namespace System;

public static class GuidExtensions
{
    /// <summary>
    /// Create short string representation from the non-empty <see cref="Guid"/>
    /// </summary>
    /// <param name="guid"></param>
    /// <returns>Base64 string encoded guid</returns>
    /// <exception cref="ArgumentException">Throws exception when Guid is empty</exception>
    public static string ToShortString(this Guid guid)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException("Guid is empty", nameof(guid));
        }

        // we can't use ids in the url with the slash
        return Convert.ToBase64String(guid.ToByteArray())[..22].Replace("/", string.Empty);
    }
}
