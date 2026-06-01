using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExcelWebApp2.Repositories.Processing
{
    public abstract class ReportProcessorBase
    {
        private static readonly Regex DecimalParseRegex = new(@"\d*\.\d*\.\d*", RegexOptions.Compiled);

        protected static decimal GetParsedDecimal(string textPrice, string fieldName = "", bool isPriceInverted = false)
        {
            var processedTextPrice = textPrice.Replace(" ", "").Replace(",", ".");
            if (DecimalParseRegex.Match(processedTextPrice).Success)
            {
                var stringNumbers = processedTextPrice.Split(".");
                processedTextPrice = string.Join(".", string.Join("", stringNumbers.SkipLast(1)), stringNumbers.Last());
            }

            var result = decimal.TryParse(processedTextPrice, NumberStyles.Any, new NumberFormatInfo() { NumberDecimalSeparator = "." }, out var decimalPrice)
                ? decimalPrice
                : throw new Exception($"Значение '{textPrice}' поля '{fieldName}' не может быть конвертировано в цену");

            return isPriceInverted ? -Math.Round(result, 2) : Math.Round(result, 2);
        }

        protected static string LabelOf<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? propertyName;
        }
    }
}
