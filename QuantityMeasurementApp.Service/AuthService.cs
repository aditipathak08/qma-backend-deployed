using QuantityMeasurementApp.Repository;
using QuantityMeasurementApp.Models;
using QuantityMeasurementApp.Utilities;
using QuantityMeasurementApp.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;

namespace QuantityMeasurementApp.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly JwtService _jwt;

        public AuthService(IUserRepository repo, JwtService jwt)
        {
            _repo = repo;
            _jwt = jwt;
        }

        // 🔐 Register new user
        public string Register(string name, string email, string password)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new QuantityMeasurementException("Email and Password are required");

            // Check if user already exists
            var existingUser = _repo.GetUserByEmail(email);
            if (existingUser != null)
                throw new QuantityMeasurementException("User already exists with this email, please login.");

            // Create secure password (salt + hash)
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword(password, salt);

            var user = new UserEntity
            {
                Name = name,
                Email = email,
                PasswordHash = hash,
                Salt = salt
            };

            // Save user
            try
            {
                _repo.AddUser(user);
            }
            catch (DbUpdateException)
            {
                throw new QuantityMeasurementException("User already exists with this email, please login.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while registering user: {ex.Message}");
            }

            return "User registered successfully";
        }

        // 🔐 Login existing user
        public string Login(string email, string password)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new QuantityMeasurementException("Email and Password are required");

            // Fetch user from DB
            var user = _repo.GetUserByEmail(email);
            if (user == null)
                throw new QuantityMeasurementException("User not found");

            // Verify password
            var hash = PasswordHasher.HashPassword(password, user.Salt);
            if (hash != user.PasswordHash)
                throw new QuantityMeasurementException("Invalid password");

            // Generate JWT token
            return _jwt.GenerateToken(email);
        }
    }
}
