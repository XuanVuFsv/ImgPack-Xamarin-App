using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PORO.Services
{
    public interface IHttpRequest
    {
        HttpClient BaseClient { get; }
        Uri BaseAddress { get; set; }
        long MaxResponseContentBufferSize { get; set; }
        HttpRequestHeaders DefaultRequestHeaders { get; }
        HttpContentHeaders DefaultContentHeaders { get; set; }
        TimeSpan Timeout { get; set; }

        #region Task

        Task<T> GetTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess = null, Func<Task> processorError = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task GetTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PostTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PostTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PostTaskAsync(string requestUri, HttpContent content, Func<object, Task> processorSuccess = null, Func<Task> processorError = null,
            object callbackObject = null, HttpMessageHandler httpMesssageHandler = null);

        Task<T> PostTaskAsync<T>(string requestUri, HttpContent content, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PostTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PostTaskAsyncCallback<T>(string requestUri, HttpContent content, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class;

        Task<T> DeleteTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess = null, Func<Task> processorError = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task DeleteTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class;

        Task<T> PutTaskAsync<T>(string requestUri, HttpContent content, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PutTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PutTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PutTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        Task PutTaskAsyncCallback<T>(string requestUri, HttpContent content, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class;

        #endregion
    }
}
