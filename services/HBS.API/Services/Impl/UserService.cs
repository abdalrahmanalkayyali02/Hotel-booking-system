using FluentValidation;
using HBS.API.Db.Repo.Interface;
using HBS.API.Dtos.Users.Registeration;
using HBS.API.Db.models;
using HBS.API.Services.Interface;
using HBS.API.Shared.enums;
using HBS.API.Shared.Result;
using System.Security.Cryptography;
using System.Text;
using HBS.API.Dtos.Users.GetById;
using HBS.API.Dtos.Users.Update;
using HBS.API.Dtos.Users.Delete;
using HBS.API.Dtos.Users.GetAll;
using HBS.API.Dtos.Users.ViewProfile;
using HBS.API.integrations.Interface;
using System.Globalization;

namespace HBS.API.Services.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly ICountriesRepository _countriesRepository;
    private readonly ICitiesRepository _citiesRepository;
    private readonly IValidator<RegisterStandardUsersDtos> _validator;
    private readonly Guid _customerRole = Guid.Parse("01a08b0c-50a4-755f-92a8-3d649593ae7f");
    private readonly ILanguagesRepository _languagesRepository;
    //private readonly Guid _languageId = Guid.Parse("01a08b0c-4db4-77f1-8af6-0d16a7f67ff3");
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IEmailService _emailService;
    private readonly IRolesRepository _rolesRepository;

    public UserService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        ICountriesRepository countriesRepository,
        ICitiesRepository citiesRepository,
        IValidator<RegisterStandardUsersDtos> validator,
        IValidator<UpdateUserRequest> updateValidator,
        IEmailService emailService,
        IRolesRepository rolesRepository,
        ILanguagesRepository languageRepository
        )
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _countriesRepository = countriesRepository;
        _citiesRepository = citiesRepository;
        _validator = validator;
        _updateValidator = updateValidator;
        _emailService = emailService;
        _rolesRepository = rolesRepository;
        _languagesRepository = languageRepository;
    }

    public async Task<Result<RegisterStandardUsersResponse>> RegisterUsers(RegisterStandardUsersDtos request)
    {
        var validationResult = _validator.Validate(request);

        //Result pattern + fluentValidation when user data validation errors occur
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => (object)group
                        .Select(error => error.ErrorMessage)
                        .ToArray()
                );
            return Result<RegisterStandardUsersResponse>.Failure(Error.Validation("User.Validation",
                "One or more validation errors occurred", errors));
        }

        //check that the country exists
        var countryExist = this._countriesRepository.GetById(request.CountryId);
        if (countryExist is null)
        {
            return Result<RegisterStandardUsersResponse>.Failure(
                Error.NotFound(
                    "Country.NotFound",
                    "The selected country was not found"
                )
            );
        }

        //check that the city exists
        var cityExist = this._citiesRepository.GetById(request.CityId);
        if (cityExist is null)
        {
            return Result<RegisterStandardUsersResponse>.Failure(
                Error.NotFound(
                    "City.NotFound",
                    "The selected city was not found"
                )
            );
        }

        //check for duplicate emails
        var normalizedEmail = request.Email.ToUpperInvariant();
        var existingEmail = this._userRepository.GetByEmail(normalizedEmail);
        if (existingEmail is not null)
        {
            return Result<RegisterStandardUsersResponse>.Failure(
                Error.Conflict("User.EmailAlreadyExists", "A User with this email already exists"));
        }

        //check for duplicate phone numbers
        var existingPhoneNumber = this._userRepository.GetByPhoneNumber(request.PhoneNumberCountryCode, request.PhoneNumberValue);
        if (existingPhoneNumber is not null)
        {
            return Result<RegisterStandardUsersResponse>.Failure(
                Error.Conflict("User.PhoneNumberAlreadyExists", "A User with this Phone Number already exists"));
        }

        //check that the city is in the country using the countryId foreign key field in cities
        if (cityExist.CountryId != countryExist.Id)
        {
            return Result<RegisterStandardUsersResponse>.Failure(Error.Conflict("City.CountryMismatch",
                "The selected city does not belong to the selected country"));
        }
        //generating the otp then hashing it
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var otpBytes = Encoding.UTF8.GetBytes(otpCode);
        var hashedOtpBytes = MD5.HashData(otpBytes);
        var hashedOtp = Convert.ToHexString(hashedOtpBytes);

        //creating a new user
        Users newUser = new Users
        {
            Id = Guid.CreateVersion7(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = normalizedEmail,
            BirthDate = request.BirthDate,
            IsEmailConfirmed = false,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumberCountryCode = request.PhoneNumberCountryCode,
            PhoneNumber = request.PhoneNumberValue,
            PhoneNumberConfirmed = false,
            RoleId = _customerRole,
            Status = UserStatus.Pending,
            VerifiedAt = null,
            CountryId = request.CountryId,
            CityId = request.CityId
        };

        //creating a new Otp
        var generatedAt = DateTime.UtcNow;

        Otp newOtp = new Otp
        {
            Id = Guid.CreateVersion7(),
            HashedOtp = hashedOtp,
            Type = OtpType.Registration,
            Target = OtpTarget.Email,
            UserId = newUser.Id,
            GeneratedAt = generatedAt,
            ExpiresAt = generatedAt.AddMinutes(5),
            IsUsed = false,
            NumberOfAttempts = 0
        };

        _userRepository.Add(newUser);
        _otpRepository.Add(newOtp);

        //sending the plain Otp
        await _emailService.SendOtpEmailAsync(
            newUser.Email,
            otpCode
        );

        var response = new RegisterStandardUsersResponse(
            newUser.Id,
            newUser.FirstName,
            newUser.LastName,
            newUser.Email,
            newUser.PhoneNumberCountryCode,
            newUser.PhoneNumber,
            newUser.BirthDate,
            newUser.PhoneNumberConfirmed,
            newUser.IsEmailConfirmed,
            newUser.RoleId,
            newUser.Status,
            newUser.CountryId,
            newUser.CityId
        );

        return Result<RegisterStandardUsersResponse>.Success(response);
    } //end Of RegisterUser method

    public Result<GetUserResponse> GetUserById(Guid userId)
    {
        var user = _userRepository.GetById(userId);
        if (user is null)
        {
            return Result<GetUserResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }
        var response = new GetUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumberCountryCode,
            user.PhoneNumber,
            user.BirthDate,
            user.PhoneNumberConfirmed,
            user.IsEmailConfirmed,
            user.RoleId,
            user.Status,
            user.CountryId,
            user.CityId
        );
        return Result<GetUserResponse>.Success(response);
    } //end of GetUserById method

    public Result<UpdateUserResponse> UpdateUserById(Guid userId, UpdateUserRequest request)
    {
        var user = _userRepository.GetById(userId);

        if (user is null)
        {
            return Result<UpdateUserResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        var validationResult = _updateValidator.Validate(request);

        //Result pattern + fluentValidation when user data validation errors occur
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => (object)group
                        .Select(error => error.ErrorMessage)
                        .ToArray()
                );
            return Result<UpdateUserResponse>.Failure(Error.Validation("User.Validation",
                "One or more validation errors occurred", errors));
        }

        //check that the country exists
        var countryExist = this._countriesRepository.GetById(request.CountryId);
        if (countryExist is null)
        {
            return Result<UpdateUserResponse>.Failure(
                Error.NotFound(
                    "Country.NotFound",
                    "The selected country was not found"
                )
            );
        }

        //check that the city exists
        var cityExist = this._citiesRepository.GetById(request.CityId);
        if (cityExist is null)
        {
            return Result<UpdateUserResponse>.Failure(
                Error.NotFound(
                    "City.NotFound",
                    "The selected city was not found"
                )
            );
        }

        //check for duplicate phone numbers
        var existingPhoneNumber = this._userRepository.GetByPhoneNumber(request.PhoneNumberCountryCode, request.PhoneNumberValue);
        if (existingPhoneNumber is not null && existingPhoneNumber.Id != user.Id)
        {
            return Result<UpdateUserResponse>.Failure(
                Error.Conflict("User.PhoneNumberAlreadyExists", "A User with this Phone Number already exists"));
        }

        //check that the city is in the country using the countryId foreign key field in cities
        if (cityExist.CountryId != countryExist.Id)
        {
            return Result<UpdateUserResponse>.Failure(Error.Conflict("City.CountryMismatch",
                "The selected city does not belong to the selected country"));
        }

        //updating the user after all request data was validated
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumberCountryCode = request.PhoneNumberCountryCode;
        user.PhoneNumber = request.PhoneNumberValue;
        user.BirthDate = request.BirthDate;
        user.CountryId = request.CountryId;
        user.CityId = request.CityId;

        _userRepository.Update(user);

        var response = new UpdateUserResponse(
            user.FirstName,
            user.LastName,
            user.PhoneNumberCountryCode,
            user.PhoneNumber,
            user.BirthDate,
            user.CountryId,
            user.CityId
        );

        return Result<UpdateUserResponse>.Success(response);
    } //end of UpdateUserById method

    public Result<SoftDeleteUserByIdResponse> SoftDeleteUserById(Guid userId)
    {
        var user = _userRepository.GetById(userId);

        if (user is null)
        {
            return Result<SoftDeleteUserByIdResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        if (user.IsDeleted)
        {
            return Result<SoftDeleteUserByIdResponse>.Failure(
                Error.Conflict(
                    "User.alreadyDeleted",
                    "The selected user is already deleted"
                )
            );
        }

        //updating the user so that his status is now Deleted
        user.Status = UserStatus.Deleted;
        _userRepository.Delete(user);

        var response = new SoftDeleteUserByIdResponse(
            user.Id,
            user.Status);

        return Result<SoftDeleteUserByIdResponse>.Success(response);
    }

    public Result<List<GetAllUsersResponse>> GetAllUsers(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            return Result<List<GetAllUsersResponse>>.Failure(
                Error.Validation(
                    "Pagination.InvalidPageNumber",
                    "Page number must be greater than zero"
                )
            );
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Result<List<GetAllUsersResponse>>.Failure(
                Error.Validation(
                    "Pagination.InvalidPageSize",
                    "page size must be between 1 and 100"
                )
            );
        }

        var users = _userRepository.GetAll(pageNumber, pageSize);

        var response = users.Select(user => new GetAllUsersResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumberCountryCode,
            user.PhoneNumber,
            user.BirthDate,
            user.PhoneNumberConfirmed,
            user.IsEmailConfirmed,
            user.RoleId,
            user.Status,
            user.CountryId,
            user.CityId))
            .ToList();

        return Result<List<GetAllUsersResponse>>.Success(response);
    }

    public Result<ViewProfileResponse> ViewProfile(Guid userId)
    {
        var languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var language = _languagesRepository.GetLanguageByCode(languageCode);
        if (language is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound("Language.NotFound",
                    "The selected language name was not found"
                )
            );
        }

        var user = _userRepository.GetById(userId); //GetById returns null if the user does not exist or if the user isDeleted

        if (user is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        var country = _countriesRepository.GetById(user.CountryId);
        if (country is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound(
                    "Country.NotFound",
                    "The selected country was not found"
                )
            );
        }

        var countryName = _countriesRepository.GetName(country.Id, language.Id);
        if (countryName is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound("CountryTranslation.NotFound",
                    "The selected country name was not found"
                )
            );
        }

        var city = _citiesRepository.GetById(user.CityId);
        if (city is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound(
                    "City.NotFound",
                    "The selected city was not found"
                )
            );
        }

        var cityName = _citiesRepository.GetName(city.Id, language.Id);
        if (cityName is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound("CityTranslation.NotFound",
                    "The selected city name was not found"
                )
            );
        }

        var role = _rolesRepository.GetById(user.RoleId);
        if (role is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound(
                    "Role.NotFound",
                    "The selected Role was not found"
                )
            );
        }

        var roleName = _rolesRepository.GetName(role.Id, language.Id);
        if (roleName is null)
        {
            return Result<ViewProfileResponse>.Failure(
                Error.NotFound("RoleTranslation.NotFound",
                    "The selected Role name was not found"
                )
            );
        }

        var response = new ViewProfileResponse(

            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumberCountryCode,
            user.PhoneNumber,
            user.BirthDate,
            roleName,
            user.Status.ToString(),
            countryName,
            cityName
            );

        return Result<ViewProfileResponse>.Success(response);
    }
} //end of class
