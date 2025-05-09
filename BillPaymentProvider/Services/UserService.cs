using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BillPaymentProvider.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // Pour la démo : comparaison simple (à remplacer par un vrai hashage sécurisé)
            return password == passwordHash;
        }
    }
}
