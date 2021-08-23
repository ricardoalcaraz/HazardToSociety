using System;
using System.Linq;

namespace HazardToSociety.Shared.Utilities
{
    public interface IQueryBuilderService
    {
        public string GetQuery(object options);
    }

    public class QueryBuilderService : IQueryBuilderService
    {
        public string GetQuery(object options)
        {
            var optionType = options.GetType();
            var propertyNames = optionType.GetProperties()
                .Select(p => new
                {
                    PropertyName = p.Name.ToLower(),
                    Value = GetPropertyValueByType(p.GetValue(options))
                }).Where(p => p.Value != null)
                .Select(p => $"{p.PropertyName}={p.Value}");
            var queryString = string.Join('&', propertyNames);

            return string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}";
        }

        private static string GetPropertyValueByType(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-dd");
            }

            return value?.ToString();
        }
    }
}