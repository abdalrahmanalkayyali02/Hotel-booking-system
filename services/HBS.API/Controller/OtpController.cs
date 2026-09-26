using HBS.API.Dtos.Otp.ResendOtp;
using HBS.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using HBS.API.Shared.Api;
using HBS.API.Dtos.Otp.VerifyOtp;

namespace HBS.API.Controller;

public class OtpController : BaseApiController
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _otpService.VerifyOtp(request);
        return HandleResult(result);
    }

    [HttpPost("resend-otp")]
    public async Task<ActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        var result = await _otpService.ResendOtp(request);
        return HandleResult(result);
    }
}