using System.Globalization;
using FluentValidation;
using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;
using HBS.API.Db.UnitOfWork.Interface;
using HBS.API.Dtos.Hotels.ChoosePrimaryImage;
using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Dtos.Hotels.GetAll;
using HBS.API.Dtos.Hotels.GetById;
using HBS.API.Dtos.Hotels.SoftDelete;
using HBS.API.Dtos.Hotels.UpdateHotel;
using HBS.API.Dtos.Hotels.UploadHotelImages;
using HBS.API.Services.Interface;
using HBS.API.Shared.enums;
using HBS.API.Shared.Result;

namespace HBS.API.Services.Impl;

public class HotelService : IHotelService
{
    private readonly IHotelsRepository _hotelRepository;
    private readonly IHotelRequestRepository _hotelRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHotelsTranslationRepository _hotelTranslationRepository;
    private readonly ILanguagesRepository _languagesRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateHotelRequest> _createHotelValidator;
    private readonly IValidator<UpdateHotelRequest> _updateHotelValidator;
    private readonly IImageService  _imageService;
    private readonly IHotelImagesRepository  _hotelImagesRepository;

    public HotelService(
        IHotelsRepository hotelRepository,
        IHotelRequestRepository hotelRequestRepository,
        IUserRepository userRepository,
        IHotelsTranslationRepository hotelTranslationRepository,
        ILanguagesRepository languagesRepository,
        IRolesRepository rolesRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateHotelRequest> createHotelValidator,
        IValidator<UpdateHotelRequest> updateHotelValidator,
        IHotelImagesRepository hotelImagesRepository,
        IImageService imageService)
    {
        _hotelRepository = hotelRepository;
        _hotelRequestRepository = hotelRequestRepository;
        _userRepository = userRepository;
        _hotelTranslationRepository = hotelTranslationRepository;
        _languagesRepository = languagesRepository;
        _rolesRepository = rolesRepository;
        _unitOfWork = unitOfWork;
        _createHotelValidator = createHotelValidator;
        _updateHotelValidator = updateHotelValidator;
        _hotelImagesRepository = hotelImagesRepository;
        _imageService = imageService;
    }

    public async Task<Result<CreateHotelResponse>> CreateHotelAsync(CreateHotelRequest request, Guid currentUserId ,string currentUserRole)
    {
      var validationResult = _createHotelValidator.Validate(request);

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
        return Result<CreateHotelResponse>.Failure(Error.Validation("User.Validation",
          "One or more validation errors occurred", errors));
      }

      Guid managerId;
      HotelStatus hotelStatus;

      if (currentUserRole == "SUPER_ADMIN")
      {
        if (request.ManagerId is null)
        {
          return Result<CreateHotelResponse>.Failure(
            Error.Validation("ManagerId.IsRequired",
              "ManagerId is Required and cannot be null"));
        }

        var manager = _userRepository.GetById(request.ManagerId.Value);
        if (manager is null)
        {
          return Result<CreateHotelResponse>.Failure(
            Error.NotFound("Manager.NotFound",
              "The manager does not exist"));
        }

        var role = _rolesRepository.GetById(manager.RoleId);
        if (role is null)
        {
          return Result<CreateHotelResponse>.Failure(
            Error.NotFound(
              "Role.NotFound",
              "The selected manager's role was not found"
            )
          );
        }

        if (role.Code != "HOTEL_ADMIN")
        {
          return Result<CreateHotelResponse>.Failure(
            Error.Conflict(
              "Role.Conflict",
              "The selected manager must be a Hotel Admin"
            )
          );
        }

        managerId = manager.Id;
        hotelStatus = HotelStatus.Approved;

      }
      else if (currentUserRole == "HOTEL_ADMIN")
      {
          managerId = currentUserId;
          hotelStatus = HotelStatus.Pending;
      }
      else
      {
        return Result<CreateHotelResponse>.Failure(
          Error.Unauthorized("Role.Unauthorized",
            "you are not allowed to create a hotel"));
      }
      var hotel = new Hotels
      {
        Id = Guid.CreateVersion7(),
        ManagerId = managerId,
        City = request.City,
        Country = request.Country,
        StarRating = request.StarRating,
        PhoneNumberCountryCode = request.PhoneNumberCountryCode,
        PhoneNumber = request.PhoneNumber,
        Email = request.Email,
        Status = hotelStatus
      };

      _hotelRepository.Add(hotel);

      foreach (var translationRequest in request.Translations)
      {
        var language = _languagesRepository.GetLanguageByCode(translationRequest.LanguageCode);
        if (language is null)
        {
          return Result<CreateHotelResponse>.Failure(
            Error.NotFound(
              "Language.NotFound",
              $"Language '{translationRequest.LanguageCode}' was not found"
            )
          );
        }

        var hotelTranslation = new HotelsTranslation
        {
          Id = Guid.CreateVersion7(),
          HotelId = hotel.Id,
          LanguageId = language.Id,
          Name = translationRequest.Name,
          Description = translationRequest.Description,
          Address = translationRequest.Address
        };

        _hotelTranslationRepository.Add(hotelTranslation);
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        var hotelRequest = new HotelRequests
        {
          Id = Guid.CreateVersion7(),
          UserId = currentUserId,
          HotelId = hotel.Id,
          Status = HotelStatus.Pending
        };

        _hotelRequestRepository.Add(hotelRequest);
      }

      await _unitOfWork.SaveChangesAsync();

      var response = new CreateHotelResponse(
        hotel.Id,
        hotelStatus
      );
      return Result<CreateHotelResponse>.Success(response);
    }

    public async Task<Result<List<GetAllHotelsResponse>>> GetAllHotels(int pageNumber, int pageSize)
    {
      if (pageNumber < 1)
      {
        return Result<List<GetAllHotelsResponse>>.Failure(
          Error.Validation(
            "Pagination.InvalidPageNumber",
            "Page number must be greater than zero"
          )
        );
      }

      if (pageSize < 1 || pageSize > 100)
      {
        return Result<List<GetAllHotelsResponse>>.Failure(
          Error.Validation(
            "Pagination.InvalidPageSize",
            "page size must be between 1 and 100"
          )
        );
      }

      var languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

      var language = _languagesRepository.GetLanguageByCode(languageCode);

      if (language is null)
      {
        return Result<List<GetAllHotelsResponse>>.Failure(
          Error.NotFound(
            "Language.NotFound",
            "The selected language was not found"
          )
        );
      }

      var hotels = _hotelRepository.GetAll(pageNumber, pageSize, language.Id);
      var response = hotels
        .Select(hotel =>
        {
          var translation =
            hotel.Translations.FirstOrDefault();

          return new GetAllHotelsResponse(
            hotel.Id,
            translation?.Name ?? "",
            translation?.Description,
            translation?.Address ?? "",
            hotel.Country,
            hotel.City,
            hotel.StarRating,
            hotel.PhoneNumberCountryCode,
            hotel.PhoneNumber,
            hotel.Email,
            hotel.ManagerId
          );
        })
        .ToList();

      return Result<List<GetAllHotelsResponse>>.Success(response);
    }

    public async Task<Result<GetHotelByIdResponse>> GetHotelByIdAsync(Guid hotelId)
    {
      var languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

      var language = _languagesRepository.GetLanguageByCode(languageCode);

      if (language is null)
      {
        return Result<GetHotelByIdResponse>.Failure(
          Error.NotFound(
            "Language.NotFound",
            "The selected language was not found"
          )
        );
      }

      var hotel = _hotelRepository.GetById(hotelId, language.Id);
      if (hotel is null)
      {
        return Result<GetHotelByIdResponse>.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The selected hotel was not found"
          )
        );
      }

      var translation = hotel.Translations.FirstOrDefault();

      if (translation is null)
      {
        return Result<GetHotelByIdResponse>.Failure(
          Error.NotFound(
            "HotelTranslation.NotFound",
            "The selected hotel does not have a translation for the current language"
          )
        );
      }

      var response = new GetHotelByIdResponse(
        hotel.Id,
        translation.Name,
        translation.Description,
        translation.Address,
        hotel.Country,
        hotel.City,
        hotel.StarRating,
        hotel.PhoneNumberCountryCode,
        hotel.PhoneNumber,
        hotel.Email,
        hotel.ManagerId
      );

      return Result<GetHotelByIdResponse>.Success(response);
    }

    public async Task<Result<UpdateHotelResponse>> UpdateHotelAsync(Guid hotelId, UpdateHotelRequest request,
      Guid currentUserId, string currentUserRole)
    {
      var validationResult = _updateHotelValidator.Validate(request);

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
        return Result<UpdateHotelResponse>.Failure(Error.Validation("User.Validation",
          "One or more validation errors occurred", errors));
      }

      var hotel = _hotelRepository.GetByIdNormalized(hotelId);

      if (hotel is null)
      {
        return Result<UpdateHotelResponse>.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The selected hotel  was not found"
          )
        );
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        if (currentUserId != hotel.ManagerId)
        {
          return Result<UpdateHotelResponse>.Failure(
            Error.Unauthorized("Hotel.Unauthorized",
              "You are not allowed to update this hotel")
          );
        }
      }else if (currentUserRole != "SUPER_ADMIN")
      {
        return Result<UpdateHotelResponse>.Failure(
          Error.Unauthorized(
            "Role.Unauthorized",
            "You are not allowed to update hotels"
          )
        );
      }

      hotel.StarRating = request.StarRating;
      hotel.PhoneNumberCountryCode = request.PhoneNumberCountryCode;
      hotel.PhoneNumber = request.PhoneNumber;
      hotel.Email = request.Email;

      foreach (var translationRequest in request.Translations)
      {
        var language = _languagesRepository.GetLanguageByCode(translationRequest.LanguageCode);

        if (language is null)
        {
          return Result<UpdateHotelResponse>.Failure(
            Error.NotFound(
              "Language.NotFound",
              $"Language '{translationRequest.LanguageCode}' was not found")
          );
        }

        var translation = _hotelTranslationRepository.GetByHotelIdAndLanguage(hotel.Id, language.Id);

        if (translation is null)
        {
          var newTranslation = new HotelsTranslation
          {
            Id = Guid.CreateVersion7(),
            HotelId = hotel.Id,
            LanguageId = language.Id,
            Name = translationRequest.Name,
            Description = translationRequest.Description,
            Address = translationRequest.Address
          };

          _hotelTranslationRepository.Add(newTranslation);
        }
        else
        {
          translation.Name = translationRequest.Name;
          translation.Description = translationRequest.Description;
          translation.Address = translationRequest.Address;
        }
      }

      await _unitOfWork.SaveChangesAsync();

      var response = new UpdateHotelResponse(
        hotel.Id,
        hotel.Status
      );
      return Result<UpdateHotelResponse>.Success(response);
    }

    public async Task<Result<SoftDeleteHotelResponse>> SoftDeleteHotelAsync(Guid hotelId, Guid currentUserId,
      string currentUserRole)
    {
      var hotel = _hotelRepository.GetByIdNormalized(hotelId);

      if (hotel is null)
      {
        return Result<SoftDeleteHotelResponse>.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The selected hotel  was not found"
          )
        );
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        if (currentUserId != hotel.ManagerId)
        {
          return Result<SoftDeleteHotelResponse>.Failure(
            Error.Unauthorized("Hotel.Unauthorized",
              "You are not allowed to update this hotel")
          );
        }
      }
      else if (currentUserRole != "SUPER_ADMIN")
      {
        return Result<SoftDeleteHotelResponse>.Failure(
          Error.Unauthorized(
            "Role.Unauthorized",
            "You are not allowed to update hotels"
          )
        );
      }

      if (hotel.IsDeleted)
      {
        return Result<SoftDeleteHotelResponse>.Failure(
          Error.Conflict("hotel.AlreadyDeleted",
            "This hotel is already deleted")
        );
      }

      hotel.IsDeleted = true;
      hotel.Status = HotelStatus.Deleted;
      await _unitOfWork.SaveChangesAsync();

      var response = new SoftDeleteHotelResponse(
        hotel.Id,
        hotel.Status
      );

      return Result<SoftDeleteHotelResponse>.Success(response);
    }

    public async Task<Result<UploadHotelImageResponse>> UploadHotelImageAsync(Guid hotelId, UploadHotelImagesRequest request, Guid currentUserId, string currentUserRole)
    {
      var hotel = _hotelRepository.GetByIdNormalized(hotelId);

      if (hotel is null)
      {
        return Result<UploadHotelImageResponse>.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The selected hotel was not found"
          )
        );
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        if (currentUserId != hotel.ManagerId)
        {
          return Result<UploadHotelImageResponse>.Failure(
            Error.Unauthorized(
              "HotelImage.Unauthorized",
              "You are not allowed to add images to this hotel"
            )
          );
        }
      }
      else if (currentUserRole != "SUPER_ADMIN")
      {
        return Result<UploadHotelImageResponse>.Failure(
          Error.Unauthorized(
            "Role.Unauthorized",
            "You are not allowed to add hotel images"
          )
        );
      }

      var uploadResult = await _imageService.UploadImageAsync(request.Image);

      var hotelImage = new HotelImages
      {
        Id = Guid.CreateVersion7(),
        HotelId = hotelId,
        ImageUrl = uploadResult.Url,
        PublicId = uploadResult.PublicId,
        IsPrimary = false
      };

      _hotelImagesRepository.Add(hotelImage);

      await _unitOfWork.SaveChangesAsync();

      var response = new UploadHotelImageResponse(
        hotelImage.Id,
        hotelImage.HotelId,
        hotelImage.ImageUrl,
        hotelImage.IsPrimary
      );

      return Result<UploadHotelImageResponse>.Success(response);
    }

    public async Task<Result> DeleteHotelImageAsync(Guid imageId, Guid currentUserId, string currentUserRole)
    {
      var hotelImage = _hotelImagesRepository.GetById(imageId);

      if (hotelImage is null)
      {
        return Result.Failure(
          Error.NotFound(
            "Hotelimage.NotFound",
            "The selected hotel image was not found"
          )
        );
      }

      var hotel = _hotelRepository.GetByIdNormalized(hotelImage.HotelId);

      if (hotel is null)
      {
        return Result.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The hotel associated with this image was not found"
          )
        );
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        if (currentUserId != hotel.ManagerId)
        {
          return Result.Failure(
            Error.Unauthorized(
              "HotelImage.Unauthorized",
              "You are not allowed to delete images from this hotel"
            )
          );
        }
      }
      else if (currentUserRole != "SUPER_ADMIN")
      {
        return Result.Failure(
          Error.Unauthorized(
            "Role.Unauthorized",
            "You are not allowed to delete hotel images"
          )
        );
      }


      await _imageService.DeleteImageAsync(hotelImage.PublicId);

      _hotelImagesRepository.Delete(hotelImage);

      await _unitOfWork.SaveChangesAsync();

      return Result.Success();
    }

    public async Task<Result> MakeImagePrimaryAsync(ChoosePrimaryImageRequest request, Guid currentUserId, string currentUserRole)
    {
      var hotelImage = _hotelImagesRepository.GetById(request.ImageId);

      if (hotelImage is null)
      {
        return Result.Failure(
          Error.NotFound(
            "HotelImage.NotFound",
            "The selected hotel image was not found"
          )
        );
      }

      var hotel = _hotelRepository.GetByIdNormalized(hotelImage.HotelId);

      if (hotel is null)
      {
        return Result.Failure(
          Error.NotFound(
            "Hotel.NotFound",
            "The hotel associated with this image was not found"
          )
        );
      }

      if (currentUserRole == "HOTEL_ADMIN")
      {
        if (currentUserId != hotel.ManagerId)
        {
          return Result.Failure(
            Error.Unauthorized(
              "HotelImage.Unauthorized",
              "You are not allowed to update images for this hotel"
            )
          );
        }
      }
      else if (currentUserRole != "SUPER_ADMIN")
      {
        return Result.Failure(
          Error.Unauthorized(
            "Role.Unauthorized",
            "You are not allowed to update hotel images"
          )
        );
      }

      if (hotelImage.IsPrimary)
      {
        return Result.Success();
      }

      var primaryImage =
        _hotelImagesRepository.GetPrimaryImageByHotelId(hotelImage.HotelId);

      if (primaryImage is not null)
      {
        primaryImage.IsPrimary = false;
        _hotelImagesRepository.Update(primaryImage);
      }

      hotelImage.IsPrimary = true;
      _hotelImagesRepository.Update(hotelImage);

      await _unitOfWork.SaveChangesAsync();

      return Result.Success();
    }
}
