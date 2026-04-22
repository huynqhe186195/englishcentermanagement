using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Application.Commons.Helpers;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Features.Users.Dtos;
using EnglishCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Application.Features.Users;

public class UserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CampusScopeHelper _campusScopeHelper;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserService(IApplicationDbContext context, IMapper mapper, CampusScopeHelper campusScopeHelper, IPasswordHasherService passwordHasherService)
    {
        _context = context;
        _mapper = mapper;
        _campusScopeHelper = campusScopeHelper;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(GetUsersPagingRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users.AsNoTracking().Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.UserName.ToLower().Contains(kw) || x.FullName.ToLower().Contains(kw) || (x.Email != null && x.Email.ToLower().Contains(kw)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<PagedResult<UserDto>> GetPagedByCampusAsync(GetUsersPagingRequestDto request, long campusId)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var query = _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.CampusId == campusId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            query = query.Where(x => x.UserName.ToLower().Contains(kw) || x.FullName.ToLower().Contains(kw) || (x.Email != null && x.Email.ToLower().Contains(kw)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<UserDetailDto> GetByIdAsync(long id)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entity == null) throw new NotFoundException("User not found.");
        return entity;
    }

    public async Task<UserDetailDto> GetByIdInCampusAsync(long id)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        return await GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(CreateUserRequestDto request)
    {
        NormalizeCreateRequest(request);

        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists) throw new BusinessException("UserName already exists.");

        await EnsureUniqueEmailAndPhoneOnCreateAsync(request);

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<long> CreateInCampusAsync(CreateUserRequestDto request, long campusId)
    {
        NormalizeCreateRequest(request);

        var userName = request.UserName.Trim();
        var exists = await _context.Users.AnyAsync(x => x.UserName == userName && !x.IsDeleted);
        if (exists)
            throw new BusinessException("UserName already exists.");

        if (string.IsNullOrWhiteSpace(request.PasswordHash))
            throw new BusinessException("Password is required.");

        await EnsureUniqueEmailAndPhoneOnCreateAsync(request);

        var entity = _mapper.Map<User>(request);
        entity.UserName = userName;
        entity.CampusId = campusId;
        entity.PasswordHash = _passwordHasherService.HashPassword(request.PasswordHash);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.IsDeleted = false;

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateUserRequestDto request)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        NormalizeUpdateRequest(request);
        await EnsureUniqueEmailAndPhoneOnUpdateAsync(id, request);

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateInCampusAsync(long id, UpdateUserRequestDto request)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        NormalizeUpdateRequest(request);
        await EnsureUniqueEmailAndPhoneOnUpdateAsync(id, request);

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteInCampusAsync(long id)
    {
        await _campusScopeHelper.EnsureUserInScopeAsync(id);
        var entity = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) throw new NotFoundException("User not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task EnsureUniqueEmailAndPhoneOnCreateAsync(CreateUserRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var emailExists = await _context.Users
                .AnyAsync(x => !x.IsDeleted && x.Email != null && x.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new BusinessException("Email already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var normalizedPhone = request.PhoneNumber.Trim();
            var phoneExists = await _context.Users
                .AnyAsync(x => !x.IsDeleted && x.PhoneNumber == normalizedPhone);
            if (phoneExists)
            {
                throw new BusinessException("PhoneNumber already exists.");
            }
        }
    }

    private async Task EnsureUniqueEmailAndPhoneOnUpdateAsync(long userId, UpdateUserRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var emailExists = await _context.Users
                .AnyAsync(x => !x.IsDeleted && x.Id != userId && x.Email != null && x.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new BusinessException("Email already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var normalizedPhone = request.PhoneNumber.Trim();
            var phoneExists = await _context.Users
                .AnyAsync(x => !x.IsDeleted && x.Id != userId && x.PhoneNumber == normalizedPhone);
            if (phoneExists)
            {
                throw new BusinessException("PhoneNumber already exists.");
            }
        }
    }

    private static void NormalizeCreateRequest(CreateUserRequestDto request)
    {
        request.UserName = request.UserName.Trim();
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        request.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
    }

    private static void NormalizeUpdateRequest(UpdateUserRequestDto request)
    {
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        request.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
    }
}
