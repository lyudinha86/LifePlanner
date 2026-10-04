using LifePlanner.Data.Entities;
using LifePlanner.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace LifePlanner.Helpers
{
    public interface IUserHelper
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(
            User user,
            string password);

        Task<SignInResult> LoginAsync(
            LoginViewModel model);

        Task LogoutAsync();

        Task<IdentityResult> ChangePasswordAsync(
            User user,
            string oldPassword,
            string newPassword);

        Task<IdentityResult> UpdateUserAsync(
            User user);

        Task CheckRoleAsync(
            string roleName);

        Task AddUserToRoleAsync(
            User user,
            string roleName);

        Task<bool> IsUserInRoleAsync(
            User user,
            string roleName);

        Task<string> GeneratePasswordResetTokenAsync(
            User user);

        Task<IdentityResult> ResetPasswordAsync(
            User user,
            string token,
            string newPassword);
    }
}