
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrasctrcuture.Db;
using MinimalApi.Domain.Interfaces;

namespace MinimalApi.Domain.Services;

public class VehicleService : IVehicle
{

    private readonly DataBaseContext _context;
    public VehicleService(DataBaseContext context)
    {
        _context = context;
    }

    public List<Vehicle> All(int? page, string? name = null, string? brand = null)
    {
       var query = _context.Vehicles.AsQueryable();
       if(!string.IsNullOrEmpty(name))
       {
         query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name}%"));
       }

       int itemsPerPage = 10;

       if(page != null)
       { 
       query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
       }

       return query.ToList();
    }

    public void Delete(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
    }

    public Vehicle? FindForID(int id)
    {
       return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();

    }

    public void Include(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
    }

    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        _context.SaveChanges();
    }
}