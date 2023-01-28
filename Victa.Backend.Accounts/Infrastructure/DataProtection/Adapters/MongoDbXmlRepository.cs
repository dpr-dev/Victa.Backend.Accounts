using System.Xml.Linq;

using Microsoft.AspNetCore.DataProtection.Repositories;

using MongoDB.Driver;

namespace Victa.Backend.Accounts.Infrastructure.DataProtection.Adapters;

public class MongoDbXmlRepository : IXmlRepository
{
    private readonly IMongoCollection<XmlEntry> _collection;

    public MongoDbXmlRepository(IMongoCollection<XmlEntry> collection)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        _collection = collection;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return _collection.Find(Builders<XmlEntry>.Filter.Empty).ToList().Select(x => XElement.Parse(x.Xml)).ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        _collection.InsertOne(new XmlEntry
        {
            Key = Guid
                .NewGuid()
                .ToString(),
            Xml = element.ToString(SaveOptions.DisableFormatting),
            FriendlyName = friendlyName
        });
    }
}
