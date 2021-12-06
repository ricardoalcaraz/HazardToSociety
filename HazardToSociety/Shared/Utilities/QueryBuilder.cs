using System;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Shared.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace HazardToSociety.Shared.Utilities
{
    public interface IQueryBuilderService
    {
        public string GetQuery(string path, object options);
        public string GetUrl(string baseUrl, object options);
    }

    public class QueryBuilderService : IQueryBuilderService
    {
        private readonly string _baseUrl;

        public QueryBuilderService(string baseUrl)
        {
            _baseUrl = baseUrl;
        }
        
        public string GetQuery(string path, object options)
        {
            var optionType = options.GetType();
            var propertyNames = optionType.GetProperties()
                .Select(p => new KeyValuePair<string, string>(p.Name, GetPropertyValueByType(p.GetValue(options))))
                .Where(p => p.Key != null && p.Value != null);
            var queryBuilder = new QueryBuilder(propertyNames);
            return path + queryBuilder;
        }

        public string GetUrl(string path, object options)
        {
            var url = $"{_baseUrl}{GetQuery(path, options)}";
            return url.ToLower();
        }

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