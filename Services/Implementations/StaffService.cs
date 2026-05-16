using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Models;
using SecureGate.Web.Services.Interfaces;
using SecureGate.Web.ViewModels;

namespace SecureGate.Web.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly AppDbContext _db;
        public StaffService(AppDbContext db) => _db = db;

        public async Task<List<Staff>> GetAllAsync() =>
            await _db.StaffMembers.OrderBy(s => s.FullName).ToListAsync();

        public async Task<Staff?> GetByIdAsync(int id) =>
            await _db.StaffMembers.FindAsync(id);

        public async Task<Staff> CreateAsync(StaffCreateViewModel model)
        {
            var staff = new Staff
            {
                FullName = model.FullName,
                Position = model.Position,
                Department = model.Department,
                Shift = model.Shift,
                Phone = model.Phone,
                AccessLevel = model.AccessLevel
            };
            _db.StaffMembers.Add(staff);
            await _db.SaveChangesAsync();
            return staff;
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _db.StaffMembers.FindAsync(id);
            if (staff != null) { _db.StaffMembers.Remove(staff); await _db.SaveChangesAsync(); }
        }
    }
}
