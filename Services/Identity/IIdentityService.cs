using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IIdentityService
    {
        Task<ApiAuthResponse> AuthenticateAsync(string username, string password);

        Task<ApiAuthResponse> AuthenticateSSOAsync(string username, SignInOtpType type);

        Task UpdateAuthInfo(ApiAuthResponse resp, string username);

        Task UpdateLastUser(string username, LoginType logintype = LoginType.SSO);

        Task<ApiAuthResponse> VerifyOTP(ApiAuthResponse autheresp, string otp);

        Task<ApiGenerateOTPResponse> GenerateOTP(ApiAuthResponse autheresp);

        Task Navigate(EbMobileSolutionData data);

        bool IsValidOTP(string otp);
    }
}
