using BonusSystem.Core.Common.IDGenerator;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using System.Net.Http;

namespace BonusSystem.Core.Services.Implementations.BFF;

public class MallBffService : IMallBffService
{
    private readonly IDataService _dataService;
    private readonly IIDGenerator _idGenerator;

    public MallBffService(IDataService dataService, IIDGenerator iDGenerator)
    {
        _dataService = dataService; 
        _idGenerator = iDGenerator;
    }
    public async Task<PagedResult<MallDto>> GetMallsAsync(int page, int pagesize)
    {
        var allMalls = (await _dataService.Malls.GetAllAsync()).ToList();
        var pagedResult = new PagedResult<MallDto>
        {
            Items = allMalls.Skip((page - 1) * pagesize).Take(pagesize).ToList(),
            TotalCount = allMalls.Count,
            Page = page,
            PageSize = pagesize
        };

        return pagedResult;
    }

    public async Task<MallDto?> GetMallByIdAsync(Guid mallId)
    {
        var mall = await _dataService.Malls.GetByIdAsync(mallId);
        if (mall == null) throw new Exception("Mall not found");
        return mall;
    }

    public async Task<MallDto?> GetMallByName(string name)
    {
        var mall = await _dataService.Malls.GetByNameAsync(name);
        return mall;
    }

    public async Task<Guid> RegisterMall(MallRegister mall_body)
    { 
        var mall_frid = _idGenerator.NewId();
        var mallDto = new MallDto
        {
            Id = Guid.NewGuid(),
            FrontendId = mall_frid,
            Name = mall_body.Name,
            City = mall_body.City,
            Region = mall_body.Region,
            WorkHours = mall_body.WorkHours

        }; 
        await _dataService.Malls.CreateAsync(mallDto);
        return mallDto.Id;
    }
}