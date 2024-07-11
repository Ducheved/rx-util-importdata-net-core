using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ImportData.Dto;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using Simple.OData.Client;

namespace ImportData.IntegrationServicesClient
{
    class Client
    {
        private static Client _instance;
        private static ODataClient _client;
        private static HttpResponseMessage _response;
        private static ODataBatch _batch;

        public static HttpResponseMessage Response { get => _response; }

        private Client(string userName, string password, string servicesUrl)
        {
            var settings = new ODataClientSettings(new Uri(servicesUrl));
            var timeout = ConfigSettingsService.GetIntParamValue(Constants.ConfigServices.RequestTimeoutParamName, "600");

            settings.RequestTimeout = new TimeSpan(0, 0, timeout);
            settings.BeforeRequest += delegate (HttpRequestMessage message)
            {
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userName, password)));
            message.Headers.Add("Authorization", "Basic " + authHeaderValue);
            };
            settings.AfterResponse += httpResonse => { _response = httpResonse; };

            _client = new ODataClient(settings);
            _batch = new ODataBatch(_client);
        }

        public static int MaxRequestsPerBatch => ConfigSettingsService.GetIntParamValue(Constants.ConfigServices.MaxRequestsPerBatch, "100");

        /// <summary>
        /// Установка параметров подключения к сервису интеграции.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="password">Пароль.</param>
        public static void Setup(string userName, string password, Logger logger)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Не указано имя пользователя для подключения к сервису интеграции.");
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("не указан пароль для подключения к сервису интеграции.");

            // Получение адреса сервиса интеграции из конфига.
            if (_instance == null)
                _instance = new Client(userName, password, ConfigSettingsService.GetConfigSettingsValueByName(Constants.ConfigServices.IntegrationServiceUrlParamName));

            logger.Info("Подготовка клиента OData.");
        }

        /// <summary>
        /// Получение сущностей.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetEntities<T>() where T : class
        {
            var data = _client.For<T>().FindEntriesAsync().Result;

            return data;
        }

        /// <summary>
        /// Получение сущностей по фильтру.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="expression">Условие фильтрации.</param>
        /// <returns>Сущность.</returns>
        public static IEnumerable<T> GetEntitiesByFilter<T>(ODataExpression expression, bool isExpand) where T : class
        {
            var query = _client.For<T>().Filter(expression);

            if (isExpand)
                query = query.Expand(ODataExpandOptions.ByValue());

            return query.FindEntriesAsync().Result;
        }

        /// <summary>
        /// Создать сущность.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="entity">Экзмемпляр сущности.</param>
        /// <returns>Созданна сущность.</returns>
        public static T CreateEntity<T>(T entity, Logger logger) where T : class
        {
            var data = _client.For<T>().Set(entity).InsertEntryAsync().Result;

            return data;
        }

        //public async static Task CreateEntityAsync<T>(T entity, ODataBatch odataBatchRequest, Logger logger) where T : class
        //{
        //    await Task.Run(() => odataBatchRequest += async client => await client.For<T>().Set(entity).InsertEntryAsync());
        //}

        public static void CreateEntityBatch<T>(T entity) where T : class
        {
            _batch += client => client.For<T>()
                .Set(entity)
                .InsertEntryAsync();
        }

        public static void CreateEntityBatch<T>(IDictionary<string, object> properties) where T : class
        {
            _batch += client => client.For<T>()
                .Set(properties)
                .InsertEntryAsync();
        }

        //public async static Task UpdateEntityAsync<T>(T entity, ODataBatch odataBatchRequest, Logger logger) where T : class
        //{
        //    //await Task.Run(() => odataBatchRequest += async client => await client.For<T>().Key(entity).Set(new { Subject = "444" }).UpdateEntryAsync());
        //    await Task.Run(() => odataBatchRequest += async client => await client.For<T>().Key(entity).Set(entity).UpdateEntryAsync());
        //}

        /// <summary>
        /// Обновить сущность.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="entity">Экзмемпляр сущности.</param>
        /// <returns>Обновленная сущность.</returns>
        public static T UpdateEntity<T>(T entity) where T : class
        {
            var data = _client.For<T>().Key(entity).Set(entity).UpdateEntryAsync().Result;

            return data;
        }

        public static void UpdateEntityBatch<T>(T entity) where T : class
        {
            _batch += client => client.For<T>().Key(entity)
                .Set(entity)
                .UpdateEntryAsync();
        }

        public static void UpdatePropertiesBatch<T>(int entityId, IDictionary<string, object> properties) where T : class
        {
            _batch += client => client.For<T>().Key(entityId)
                .Set(properties)
                .UpdateEntryAsync();
        }

        public static void CreateVersion<T>(int entityId, int versionId, int verNumber, Attachment attachment) where T : IElectronicDocuments
        {
            _batch += client => client.For<T>().Key(entityId)
                .NavigateTo("Versions")
                .Set(new { Id = versionId, Number = verNumber, attachment.AssociatedApplication })
                .InsertEntryAsync();
        }

        public static void CreateBody<T>(int entityId, int versionId, Attachment attachment) where T : IElectronicDocuments
        {
            _batch += client => client.For<T>().Key(entityId)
                .NavigateTo("Versions").Key(versionId)
                .NavigateTo("Body")
                .Set(new { Value = attachment.Body })
                .InsertEntryAsync();
        }

        /// <summary>
        /// Получение экземпляра клиента OData.
        /// </summary>
        /// <returns>Simple.OData.</returns>
        public static ODataClient Instance()
        {
            return _client;
        }

        public async static Task SaveBatchAsync()
        {
            await _batch.ExecuteAsync();

            _batch = new ODataBatch(_client);
        }

    }
}
