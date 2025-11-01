using Newtonsoft.Json;
namespace Teams.APP.Layer.Helpers;

public static class ProjectHelper
{
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value == null)
            {
                throw new JsonSerializationException("DateTime value is null.");
            }
            var dt = (DateTime)reader.Value;
            return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) => serializer.Serialize(writer, value);
    }

    /// <summary>
    /// Désérialise un message JSON en un DTO typé avec les paramètres de conversion personnalisés.
    /// </summary>
    /// <typeparam name="T">Le type du DTO attendu</typeparam>
    /// <param name="message">Le message JSON</param>
    /// <returns>L'objet désérialisé de type T</returns>
    public static T? ConvertJsonMessageIntoDto<T>(this string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Le message JSON est vide", nameof(message));

        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        return JsonConvert.DeserializeObject<T>(message, settings);
    }

    public static async Task<T> GetDtoConverted<T>(this string message)
    {
        var dto = message.ConvertJsonMessageIntoDto<T>();
        if (dto == null) throw new InvalidOperationException("Failed to retrieve project association data"); return dto;
    }
}