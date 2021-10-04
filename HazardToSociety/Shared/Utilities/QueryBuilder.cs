using System;
using System.Linq;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Shared.Utilities
{
    public interface IQueryBuilderService
    {
        public string GetQuery(object options);
        public string GetUrl(string baseUrl, object options);
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

        public string GetUrl(string baseUrl, object options) => $"{baseUrl}{GetQuery(options)}";

        private static string GetPropertyValueByType(object value)
        {
            return value switch
            {
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd"),
                _ => value?.ToString()
            };
        }
    }
}