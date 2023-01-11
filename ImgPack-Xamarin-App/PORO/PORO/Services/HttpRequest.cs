using Plugin.Connectivity;
using PORO.Untilities;
using PORO.Views.Popups;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PORO.Services
{
    public class HttpRequest : IHttpRequest
    {
        #region Properties

        public int TimeoutSeconds = 120;

        public TimeSpan Timeout
        {
            get => BaseClient.Timeout;
            set => BaseClient.Timeout = value;
        }

        private Serializer _crossJsonSerializer;
        public Serializer CrossJsonSerializer
            => LazyInitializer.EnsureInitialized(ref _crossJsonSerializer, () => new Serializer());

        public HttpClient BaseClient
        {
            get;
            private set;
        }

        public Uri BaseAddress
        {
            get => BaseClient.BaseAddress;
            set => BaseClient.BaseAddress = value;
        }

        public long MaxResponseContentBufferSize
        {
            get => BaseClient.MaxResponseContentBufferSize;
            set => BaseClient.MaxResponseContentBufferSize = value;
        }

        public HttpRequestHeaders DefaultRequestHeaders => BaseClient.DefaultRequestHeaders;

        public HttpContentHeaders DefaultContentHeaders { get; set; }

        public CancellationTokenSource CancelTokenSource { get; set; }

        #endregion

        #region constructor

        public HttpRequest()
        {
            //BaseClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
            //Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

            HttpClientHandler clientHandler = new HttpClientHandler() { UseCookies = false };
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            //clientHandler.CookieContainer = new CookieContainer();

            // Pass the handler to httpclient(from you are calling api)
            BaseClient = new HttpClient(clientHandler) { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        }

        #endregion

        #region Origin method

        #region GetAsync

        private async Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await MessagePopup.Instance.Show("Network Error");
                return null;
            }

            //IEnumerable<string> headersTemp = new ObservableCollection<string>();
            //App.HttpHeaders.TryGetValues("Set-Cookie", out headersTemp);

            //var headers = new ObservableCollection<string>(headersTemp);

            //var cookieContainer = new CookieContainer(); 
            //cookieContainer.Add(new Cookie("Cookie", headers[0]));
            //cookieContainer.Add(new Cookie("Cookie", headers[1]));

            //HttpClientHandler clientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
            //clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            //// Pass the handler to httpclient(from you are calling api)
            //BaseClient = new HttpClient(clientHandler) { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };

            return await BaseClient.GetAsync(requestUri, completionOption, cancellationToken);
        }

        #endregion

        #region PostAsync

        private async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await MessagePopup.Instance.Show("Network Error");
                return null;
            }

            //CustomDelegatingHandler customDelegatingHandler = new CustomDelegatingHandler();
            //BaseClient = HttpClientFactory.Create(customDelegatingHandler);

            return await BaseClient.PostAsync(requestUri, content, cancellationToken);
        }

        #endregion

        #region Delete Async

        private async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await MessagePopup.Instance.Show("Network Error");
                return null;
            }

            return await BaseClient.DeleteAsync(requestUri, cancellationToken);
        }

        #endregion

        #region Put Async

        private async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await MessagePopup.Instance.Show("Network Error");
                return null;
            }

            return await BaseClient.PutAsync(requestUri, content, cancellationToken);
        }

        #endregion

        #endregion

        #region GetAsync

        /// <summary>
        /// get with call back is a task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <returns></returns>

        public async Task<T> GetTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess = null,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class
        {
#if DEBUG
            Debug.WriteLine($"Get method with url: {requestUri}");
#endif
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await GetAsync(requestUri,
                    HttpCompletionOption.ResponseContentRead, CancelTokenSource.Token);

                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                    return null;
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return null;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                            return null;
                        }
                        else
                        {
                            if (processorSuccess != null)
                                await processorSuccess(result);
                            return result;
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken == CancelTokenSource.Token)
                {
                    if (processorError != null) await processorError();
                    var token = CancelTokenSource.Token;
#if DEBUG
                    Debug.WriteLine(
                        $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
#endif
                }
                else
                {
                    if (processorError != null) await processorError();
#if DEBUG
                    Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
#endif
                }

                return null;
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
                return null;
            }
            catch (Exception exception)
            {
                if (processorError != null) await processorError();
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                return null;
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        /// <summary>
        /// get with callback object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <param name="callbackObject"></param>
        /// <returns></returns>
        public async Task GetTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class
        {
#if DEBUG
            Debug.WriteLine($"Get method with url: {requestUri}");
#endif
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await GetAsync(requestUri, HttpCompletionOption.ResponseContentRead,
                    CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result, callbackObject);
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken == CancelTokenSource.Token)
                {
                    if (processorError != null) await processorError();
                    var token = CancelTokenSource.Token;
#if DEBUG
                    Debug.WriteLine(
                        $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
#endif
                }
                else
                {
                    if (processorError != null) await processorError();
#if DEBUG
                    Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
#endif
                }
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
                if (processorError != null) await processorError();
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        #endregion

        #region PostAsync

        public async Task PostTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var stringContent = keyvalues.KeyValuePairToStringContent(requestUri);
                var responseMessage = await PostAsync(requestUri, stringContent, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            if (processorSuccess != null) await processorSuccess(result);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");

            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PostTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PostAsync(requestUri, content, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            if (processorSuccess != null) await processorSuccess(result);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();

            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PostTaskAsync(string requestUri, HttpContent content, Func<object, Task> processorSuccess = null,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null)
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            var responseMessage = await PostAsync(requestUri, content, CancelTokenSource.Token);

            try
            {
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    await processorSuccess(callbackObject);
                }
                else
                {
                    if (processorError != null)
                    {
                        if (await IsTokenExpired(responseMessage))
                        {
                            await TokenExpiredExecute();
                        }

                        await processorError();
                    }
                }

            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, " +
                          $"message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task<T> PostTaskAsync<T>(string requestUri, HttpContent content,
            HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PostAsync(requestUri, content, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    return null;
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return null;
                    }

                    //responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        App.HttpHeaders = responseMessage.Headers;

                        return result;
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, " +
                          $"message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                return null;
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");

                return null;
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                return null;
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PostTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var stringContent = keyvalues.KeyValuePairToStringContent(requestUri);
                var responseMessage = await PostAsync(requestUri, stringContent, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var seObj = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                        if (seObj == null)
                        {
#if DEBUG
                            Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
#endif
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(seObj, callbackObject);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();

            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine(
                    $"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PostTaskAsyncCallback<T>(string requestUri, HttpContent content,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null,
            object callbackObject = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PostAsync(requestUri, content, CancelTokenSource.Token);

                if (await IsTokenExpired(responseMessage))
                {
                    await TokenExpiredExecute();
                    return;
                }

                responseMessage = responseMessage.EnsureSuccessStatusCode();

                using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    var seObj = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                    if (seObj == null)
                    {
#if DEBUG
                        Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
#endif
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        await processorSuccess(seObj, callbackObject);
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine(
                    $"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        #endregion

        #region DeleteAsync

        /// <summary>
        /// delele task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <returns></returns>
        public async Task<T> DeleteTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess = null,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
#if DEBUG
            Debug.WriteLine($"Delete method with url: {requestUri}");
#endif
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await DeleteAsync(requestUri, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null)
                    {
                        await processorError();
                    }
                    return null;
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return null;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null)
                            {
                                await processorError();
                            }
                            return null;
                        }
                        else
                        {
                            if (processorSuccess != null)
                            {
                                await processorSuccess(result);
                            }
                            return result;
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken == CancelTokenSource.Token)
                {
                    if (processorError != null) await processorError();
                    var token = CancelTokenSource.Token;
#if DEBUG
                    Debug.WriteLine(
                        $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
#endif
                }
                else
                {
                    if (processorError != null) await processorError();
#if DEBUG
                    Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
#endif
                }
                return null;
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");

                return null;
            }
            catch (Exception exception)
            {
                if (processorError != null) await processorError();
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                return null;
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        /// <summary>
        /// delete task with call back
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <param name="callbackObject"></param>
        /// <returns></returns>
        public async Task DeleteTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null, HttpMessageHandler httpMesssageHandler = null)
            where T : class
        {
#if DEBUG
            Debug.WriteLine($"Delete method with url: {requestUri}");
#endif
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await DeleteAsync(requestUri, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    if (await IsTokenExpired(responseMessage))
                    {
                        await TokenExpiredExecute();
                        return;
                    }

                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result, callbackObject);
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken == CancelTokenSource.Token)
                {
                    if (processorError != null) await processorError();
                    var token = CancelTokenSource.Token;
#if DEBUG
                    Debug.WriteLine(
                        $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
#endif
                }
                else
                {
                    if (processorError != null) await processorError();
#if DEBUG
                    Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
#endif
                }
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
                if (processorError != null) await processorError();
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        #endregion

        #region PutAsync

        public async Task<T> PutTaskAsync<T>(string requestUri, HttpContent content,
            HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PutAsync(requestUri, content, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    return null;
                }
                else
                {
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        return result;
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, " +
                          $"message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                return null;
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");

                return null;
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                return null;
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PutTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null,
            HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var stringContent = keyvalues.KeyValuePairToStringContent(requestUri);
                var responseMessage = await PutAsync(requestUri, stringContent, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();

            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PutTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PutAsync(requestUri, content, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var result = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PutTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null,
            object callbackObject = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var stringContent = keyvalues.KeyValuePairToStringContent(requestUri);
                var responseMessage = await PutAsync(requestUri, stringContent, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    using (responseMessage = responseMessage.EnsureSuccessStatusCode())
                    {
                        var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                        var seObj = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                        if (seObj == null)
                        {
#if DEBUG
                            Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
#endif
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(seObj, callbackObject);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine(
                    $"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        public async Task PutTaskAsyncCallback<T>(string requestUri, HttpContent content,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null,
            object callbackObject = null, HttpMessageHandler httpMesssageHandler = null) where T : class
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelTokenSource.CancelAfter(Timeout);
            try
            {
                var responseMessage = await PutAsync(requestUri, content, CancelTokenSource.Token);
                if (responseMessage == null)
                {
                    if (processorError != null) await processorError();
                }
                else
                {
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    using (var jsonStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var seObj = CrossJsonSerializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                        if (seObj == null)
                        {
#if DEBUG
                            Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
#endif
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(seObj, callbackObject);
                        }
                    }
                }
            }
            catch (TaskCanceledException taskCanceledException)
            {
#if DEBUG
                Debug.WriteLine(
                    taskCanceledException.CancellationToken == CancelTokenSource.Token
                        ? $"A real cancellation, triggered by the caller, token is: {CancelTokenSource.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                        : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            catch (UnauthorizedAccessException unAuEx)
            {
                await MessagePopup.Instance.Show("Invalid API call");
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine(
                    $"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
#endif
                if (processorError != null) await processorError();
            }
            finally
            {
                CancelTokenSource.Dispose();
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose of everything
        /// </summary>
        public static void Dispose()
        {

        }

        #endregion

        #region Token Expired

        private async Task<bool> IsTokenExpired(object response)
        {
            try
            {
                if (((HttpResponseMessage)response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        var result = await ((HttpResponseMessage)response).Content.ReadAsStreamAsync();
                        var a = (HttpResponseMessage)response;
                        var b = a.EnsureSuccessStatusCode();
                        //var res = CrossJsonSerializer.DeserializeFromJsonStream<CrossPayJObjectResponse>(result, "");

                        //if (res.ErrorCode == Define.ErrorCodeTokenExpiration)
                        return true;
                    }
                    catch (Exception)
                    {
                        //var result = await ((HttpResponseMessage)response).Content.ReadAsStreamAsync();
                        //var res = CrossJsonSerializer.DeserializeFromJsonStream<CrossPayJArrayResponse>(result, "");

                        //if (res.ErrorCode == Define.ErrorCodeTokenExpiration)
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e);
#endif
            }
            return false;
        }

        private async Task TokenExpiredExecute()
        {
            try
            {
                var vm = ManagerPage.GetCurrentPageBaseViewModel();

                var pageViewModel = vm.ToString();

                //await vm.LogoutExecute();
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e);
#endif
            }
        }

        #endregion
    }
}
