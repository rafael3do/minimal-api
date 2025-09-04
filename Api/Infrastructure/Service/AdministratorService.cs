using System.Data.Common;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

namespace MinimalApi.Domain.Service;

public class AdministratorService : IAdministratorService
{
    private readonly DatabaseContext _context;
    
    public AdministratorService(DatabaseContext context)
    {
        _context = context;
    }

    public List<Administrator> All(int? pagina)
    {
        var query = _context.Administrators.AsQueryable();

        int itensPorPagina = 10;

        if(pagina != null)
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return query.ToList();
    }

    public Administrator? BuscaPorId(int id)
    {
        return _context.Administrators.Where(a => a.Id == id).FirstOrDefault();
    }

    public Administrator Incluir(Administrator administrator)
    {
        _context.Administrators.Add(administrator);
        _context.SaveChanges();

        return administrator;
    }

    public Administrator? Login(LoginDTO loginDTO)
    {
        var adm = _context.Administrators.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }
}