using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AuthServerClient
{
    /// <summary>
    /// Client library for integrating .NET Framework applications with AuthServer
    /// </summary>
    public class AuthServerClient : IDisposable
    {
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialize AuthServer client with configuration from App.config/Web.config
        /// </summary>
        public AuthServerClient()
        {
            _baseUrl = ConfigurationManager.AppSettings["AuthServer:BaseUrl"]
                ?? throw new InvalidOperationException("AuthServer:BaseUrl not configured");
            _clientId = ConfigurationManager.AppSettings["AuthServer:ClientId"]
                ?? throw new InvalidOperationException("AuthServer:ClientId not configured");
            _clientSecret = ConfigurationManager.AppSettings["AuthServer:ClientSecret"]
                ?? throw new InvalidOperationException("AuthServer:ClientSecret not configured");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Initialize AuthServer client with custom configuration
        /// </summary>
        public AuthServerClient(string baseUrl, string clientId, string clientSecret)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            var loginRequest = new
            {
                email = email,
                password = password,
                clientId = _clientId,
                clientSecret = _clientSecret
            };

            var result = await PostAsync<LoginResponse>("/Authentication/Login", loginRequest);
            return result;
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required", nameof(refreshToken));

            var refreshRequest = new
            {
                refreshToken = refreshToken,
                clientId = _clientId,
                clientSecret = _clientSecret
            };

            var result = await PostAsync<LoginResponse>("/Authentication/RefreshToken", refreshRequest);
            return result;
        }

        /// <summary>
        /// Validate if an access token is still valid
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return false;

            try
            {
                var tempClient = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
                tempClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await tempClient.GetAsync("/Authentication/ValidateToken");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Logout and invalidate refresh token
        /// </summary>
        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            try
            {
                var logoutRequest = new { refreshToken = refreshToken };
                await PostAsync<object>("/Authentication/Logout", logoutRequest);
            }
            catch
            {
                // Ignore logout errors
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required", nameof(accessToken));
            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new ArgumentException("Current password is required", nameof(currentPassword));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password is required", nameof(newPassword));

            var changePasswordRequest = new
            {
                currentPassword = currentPassword,
                newPassword = newPassword
            };

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var result = await PostAsync<bool>("/Authentication/ChangePassword", changePasswordRequest);
                return result;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        public async Task<bool> RequestPasswordResetAsync(string email, Guid? tenantId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            var request = new
            {
                email = email,
                tenantId = tenantId
            };

            try
            {
                await PostAsync<object>("/Authentication/RequestPasswordReset", request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required", nameof(token));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password is required", nameof(newPassword));

            var request = new
            {
                token = token,
                newPassword = newPassword
            };

            try
            {
                await PostAsync<object>("/Authentication/ResetPassword", request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create authenticated HTTP client for making API calls
        /// </summary>
        public AuthenticatedHttpClient CreateAuthenticatedClient(string accessToken, string refreshToken)
        {
            return new AuthenticatedHttpClient(this, accessToken, refreshToken);
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorResult = JsonConvert.DeserializeObject<ApiResult<object>>(responseContent);
                throw new AuthServerException(errorResult?.Message ?? "Request failed", response.StatusCode);
            }

            var result = JsonConvert.DeserializeObject<ApiResult<T>>(responseContent);

            if (result == null || !result.Success)
            {
                throw new AuthServerException(result?.Message ?? "Request failed");
            }

            return result.Data;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// HTTP client that automatically refreshes tokens
    /// </summary>
    public class AuthenticatedHttpClient : IDisposable
    {
        private readonly AuthServerClient _authClient;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _refreshToken;

        internal AuthenticatedHttpClient(AuthServerClient authClient, string accessToken, string refreshToken)
        {
            _authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _refreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));

            _httpClient = new HttpClient();
            UpdateAuthorizationHeader();
        }

        public string AccessToken => _accessToken;
        public string RefreshToken => _refreshToken;

        private void UpdateAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenInternalAsync();
                response = await _httpClient.GetAsync(url);
            }

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            var response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenInternalAsync();
                response = await _httpClient.PostAsync(url, content);
            }

            return response;
        }

        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        {
            var response = await _httpClient.PutAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenInternalAsync();
                response = await _httpClient.PutAsync(url, content);
            }

            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenInternalAsync();
                response = await _httpClient.DeleteAsync(url);
            }

            return response;
        }

        private async Task RefreshTokenInternalAsync()
        {
            var result = await _authClient.RefreshTokenAsync(_refreshToken);
            _accessToken = result.AccessToken;
            _refreshToken = result.RefreshToken;
            UpdateAuthorizationHeader();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    #region DTOs and Models

    public class LoginResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("tokenType")]
        public string TokenType { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonProperty("user")]
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("tenantId")]
        public Guid? TenantId { get; set; }

        [JsonProperty("tenantName")]
        public string TenantName { get; set; }

        [JsonProperty("isSystemAdmin")]
        public bool IsSystemAdmin { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class ApiResult<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }

    public class AuthServerException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }

        public AuthServerException(string message) : base(message)
        {
        }

        public AuthServerException(string message, System.Net.HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public AuthServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    #endregion
}
