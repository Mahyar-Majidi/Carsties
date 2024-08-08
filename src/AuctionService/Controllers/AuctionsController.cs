using System;
using AuctionService.Data;
using AuctionService.Data.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController: ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions() {
        
        var auctions = await _context.Auctions
            .Include(a => a.Item)
            .OrderBy(a => a.Item.Make)
            .ToListAsync();
        
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id) {

        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(q => q.Id == id);
        
        if (auction == null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task <ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto) {

        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO: add current user as seller
        auction.Seller = "test";

        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not add information to the DataBase!");

        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult>UpdateAcution(Guid id, UpdateAuctionDto updateAuctionDto) {
        var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(q => q.Id == id);
        if (auction == null) return NotFound();

        // TODO: check seller == usrename

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok();
        return BadRequest("There is a problem with the updata operation! update had not been succeed.");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id){
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();

        // TODO: check saller == username

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not update DataBase!");
        return Ok();
    }
}
