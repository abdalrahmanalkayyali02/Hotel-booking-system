using HBS.API.Shared.Result;
using HBS.API.Dtos.Otp.VerifyOtp;
using HBS.API.Dtos.Otp.ResendOtp;

namespace HBS.API.Services.Interface;

public interface IOtpService
{
    public Task<Result<VerifyOtpResponse>> VerifyOtp(VerifyOtpRequest request);
    public Task<Result<ResendOtpResponse>> ResendOtp(ResendOtpRequest request);
}