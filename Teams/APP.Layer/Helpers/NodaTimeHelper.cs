// using System;
// using NodaTime;
// using NodaTime.Text;

// namespace Teams.APP.Layer.Helpers;

// public static class NodaTimeHelper
// {
//     private static readonly DateTimeZoneProvider TimeZones = DateTimeZoneProviders.Tzdb;

//     /// <summary>
//     /// Parse une date JSON de manière agnostique et retourne un Instant (UTC).
//     /// </summary>
//     /// <param name="dateString">Date reçue en JSON</param>
//     /// <returns>Instant en UTC</returns>
//     public static Instant ParseJsonDateToUtcInstant(string dateString)
//     {
//         if (string.IsNullOrWhiteSpace(dateString))
//             throw new ArgumentNullException(nameof(dateString));

//         // Tente de parser en ISO-8601 avec NodaTime
//         var parseResult = InstantPattern.ExtendedIso.Parse(dateString);
//         if (parseResult.Success)
//         {
//             return parseResult.Value; // déjà en UTC
//         }

//         // Tente de parser en DateTimeOffset et convertir en Instant
//         if (DateTimeOffset.TryParse(dateString, out var dto))
//         {
//             return Instant.FromDateTimeOffset(dto.ToUniversalTime());
//         }

//         // Tente de parser en DateTime et considérer UTC
//         if (DateTime.TryParse(dateString, out var dt))
//         {
//             var dtUtc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
//             return Instant.FromDateTimeUtc(dtUtc);
//         }

//         throw new FormatException($"Impossible de parser la date JSON : {dateString}");
//     }

//     /// <summary>
//     /// Convertit un Instant UTC vers un fuseau horaire spécifique et retourne un ZonedDateTime.
//     /// </summary>
//     /// <param name="instantUtc">Instant en UTC</param>
//     /// <param name="timeZoneId">Ex: "Europe/Paris"</param>
//     /// <returns>ZonedDateTime dans le fuseau spécifié</returns>
//     public static ZonedDateTime ConvertInstantToTimeZone(Instant instantUtc, string timeZoneId)
//     {
//         if (!TimeZones.Ids.Contains(timeZoneId))
//             throw new TimeZoneNotFoundException($"Fuseau horaire inconnu : {timeZoneId}");

//         var tz = TimeZones[timeZoneId];
//         return instantUtc.InZone(tz);
//     }

//     /// <summary>
//     /// Retourne la date/heure UTC sous forme DateTimeOffset pour compatibilité .NET classique
//     /// </summary>
//     public static DateTimeOffset InstantToDateTimeOffset(Instant instantUtc)
//     {
//         return instantUtc.ToDateTimeOffset();
//     }
// }
