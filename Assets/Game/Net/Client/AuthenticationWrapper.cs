using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Game.Net.Client
{
    public static class AuthenticationWrapper
    {
        public static AuthState AuthState { get; private set; }

        private static IAuthenticationService service => AuthenticationService.Instance;

        private const int AuthenticatingPeriod = 200;
        private const int SignInPeriod = 1000;
        private const int DefaultAuthenticationAttempts = 5;

        private static async Task Authenticating()
        {
            while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
            {
                await Task.Delay(AuthenticatingPeriod);
            }
        }
        
        private static async Task SignInAnonymouslyAsync(int attempts)
        {
            var attempt = 0;
            while (AuthState == AuthState.Authenticating && attempt < attempts)
            {
                try
                {
                    await service.SignInAnonymouslyAsync();

                    if (service.IsSignedIn && service.IsAuthorized)
                    {
                        AuthState = AuthState.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException authException)
                {
                    Debug.LogError(authException);
                    AuthState = AuthState.Error;
                }
                catch (RequestFailedException requestException)
                {
                    Debug.LogError(requestException);
                    AuthState = AuthState.Error;
                }

                attempt++;
                await Task.Delay(SignInPeriod);
            }

            if (AuthState != AuthState.Authenticated)
            {
                Debug.LogWarning($"Player was not signed in successfully after {attempts} attempts.");
                AuthState = AuthState.TimeOut;
            }
        }
        
        public static async Task<AuthState> Authenticate(int attempts = DefaultAuthenticationAttempts)
        {
            if (AuthState == AuthState.Authenticated)
            {
                return AuthState;
            }

            if (AuthState == AuthState.Authenticating)
            {
                Debug.LogWarning("Already authenticating...");
                
                await Authenticating();

                return AuthState;
            }
            
            AuthState = AuthState.Authenticating;

            await SignInAnonymouslyAsync(attempts);

            if (AuthState != AuthState.Authenticated)
            {
                //
            }

            return AuthState;
        }
    }

    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimeOut,
    }
}
