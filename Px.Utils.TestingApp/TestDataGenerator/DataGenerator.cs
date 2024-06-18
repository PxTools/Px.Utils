using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.TestingApp.TestDataGenerator
{
    internal static class DataGenerator
    {
        internal static DecimalDataValue[] GenerateDecimnalDataValues(int count)
        {
            Random random = new(DateTime.UtcNow.Microsecond);
            DecimalDataValue[] values = new DecimalDataValue[count];
            for (int i = 0; i < count; i++)
            {
                decimal value = (decimal)Math.Pow(10, random.Next(0, 10)) * (decimal)random.NextDouble();
                values[i] = new DecimalDataValue(value, Models.Data.DataValueType.Exists);
            }

            return values;
        }

        internal static double[] GenerateDoubles(int count)
        {
            Random random = new(DateTime.UtcNow.Microsecond);
            double[] values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = Math.Pow(10, random.Next(0, 10)) * random.NextDouble();
            }

            return values;
        }
    }
}
