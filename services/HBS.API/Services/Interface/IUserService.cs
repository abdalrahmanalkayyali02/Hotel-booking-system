using HBS.API.Dtos.Users.Registeration;
using HBS.API.Shared.Result;
using HBS.API.Dtos.Users.GetById;
using HBS.API.Dtos.Users.Update;
using HBS.API.Dtos.Users.Delete;
using HBS.API.Dtos.Users.GetAll;
using HBS.API.Dtos.Users.ViewProfile;

namespace HBS.API.Services.Interface;

public interface IUserService
{
    public Task<Result<RegisterStandardUsersResponse>> RegisterUsers(RegisterStandardUsersDtos request);
    public Result<GetUserResponse> GetUserById(Guid userId);
    public Result<UpdateUserResponse> UpdateUserById(Guid userId, UpdateUserRequest request);
    public Result<SoftDeleteUserByIdResponse> SoftDeleteUserById(Guid userId);
    public Result<List<GetAllUsersResponse>> GetAllUsers(int pageNumber, int pageSize);
    public Result<ViewProfileResponse> ViewProfile(Guid userId); //user id from the access token
}