using System.Security.Claims;
using HBS.API.Dtos.Users.Registeration;
using HBS.API.Dtos.Users.Update;
using HBS.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using HBS.API.Shared.Api;
using Microsoft.AspNetCore.Authorization;

namespace HBS.API.Controller;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register-customer")]
    public async Task<ActionResult> Register([FromBody] RegisterStandardUsersDtos request)
    {
        var result = await _userService.RegisterUsers(request);

        return HandleResult(result);
        /*if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(result.StatusCode, result.Error);*/
    }

    [HttpGet("get-by-id/{userId:guid}")]
    public ActionResult GetById(Guid userId)
    {
        var result = _userService.GetUserById(userId);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("update-user/{userId:guid}")]
    public ActionResult UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var result = _userService.UpdateUserById(userId, request);

        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("delete-user/{userId:guid}")]
    public ActionResult DeleteUser(Guid userId)
    {
        var result = _userService.SoftDeleteUserById(userId);

        return HandleResult(result);
    }

    [HttpGet("get-all-users/{pageNumber:int}/{pageSize:int}")]
    public ActionResult GetAllUsers(int pageNumber, int pageSize)
    {
        var result = _userService.GetAllUsers(pageNumber, pageSize);
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public ActionResult ViewProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = _userService.ViewProfile(userId);
        
        return HandleResult(result);
    }

}
