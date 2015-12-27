using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Core.Notification
{
    public static class GenericExtensions
    {
        public static byte[] Serialize<TItem>(this TItem item) where TItem : class
        {
            if (item == null)
            {
                return new byte[0];
            }
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, item);
                return stream.ToArray();
            }
        }

        public static TItem Deserialize<TItem>(this byte[] item) where TItem : class
        {
            if (item == null || item.Length == 0)
            {
                return null;
            }
            var deserializer = new BinaryFormatter();
            using (var stream = new MemoryStream(item))
            {
                return (TItem)deserializer.Deserialize(stream);
            }
        }
    }
}
