﻿using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Enums
{
    /// <summary>
    /// Enum for the period between two time dimension values
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TimeDimensionInterval
    {
        Year,
        HalfYear,
        Quarter,
        Month,
        Week,
        Other,
        Irregular
    }
}
