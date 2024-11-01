using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;
public interface IVehicle
{
    List<Vehicle> All(int? page, string? name = null, string? brand = null);
    Vehicle? FindForID(int id);
    void Include(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}
