using System;
using System.Text;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.Services.Authentication.PlayerAccounts.Samples
{
    public class UnityPlayerAccountsUIExample : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI m_ExceptionText;

        /// <summary>
        /// Initialize unity services and setup event handlers.
        /// </summary>
        private async void Start()
        {
            await UnityServices.InitializeAsync();
            PlayerAccountService.Instance.SignedIn += SignInWithUnity;
        }

        /// <summary>
        /// Start the browser-based Unity Player Accounts sign-in flow. If successful this will provide
        /// a Unity Player Accounts access token, which can be used to sign in to the Unity Authentication service.
        /// </summary>
        async void StartSignInAsync()
        {
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                SignInWithUnity();
                return;
            }

            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetException(ex);
            }
        }

        /// <summary>
        /// Sign in to Unity Authentication using the access token from Unity Player Accounts.
        /// This will be called after the player has successfully signed in to Unity Player Accounts.
        /// </summary>
        private async void SignInWithUnity()
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                SceneManager.LoadScene(1);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
                SetException(ex);
            }
        }


        private void SetException(Exception ex)
        {
            m_ExceptionText.text = ex != null ? $"{ex.GetType().Name}: {ex.Message}" : "";
        }
    }
}