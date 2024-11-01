
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrasctrcuture.Db;
using MinimalApi.Domain.Interfaces;

namespace MinimalApi.Domain.Services;

public class AdministratorService : IAdministrator 
{

    private readonly DataBaseContext _context;
    public AdministratorService(DataBaseContext context)
    {
        _context = context;
    }
    public Administrator Login(LoginDTO loginDTO)
    {   
        var adm = _context.Administrators.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault();
        return adm;
    }  
    public Administrator Include(Administrator adm)
    {
        _context.Administrators.Add(adm);
        _context.SaveChanges();

        return adm;
    }

    public List<Administrator> All(int? page)
    {
        var query = _context.Administrators.AsQueryable();

       int itemsPerPage = 10;

       if(page != null)
       { 
       query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
       }

       return query.ToList();
    }

    public Administrator? FindForID(int id)
    {
        return _context.Administrators.Where(v => v.Id == id).FirstOrDefault();
    }
}