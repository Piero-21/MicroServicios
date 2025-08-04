using AutoMapper;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using UserService.Models;
using UserService.Models.Dtos;
using UserService.Repository.IRepository;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMapper _mapper;

        public UserRepository(IDbConnection db, IMapper mapper)
        {
            _dbConnection = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbConnection.QueryAsync<User>("SELECT * FROM Users WHERE Deleted = 0");
        }

        public async Task<bool> AddAsync(User user)
        {
            var sql = @"
                        INSERT INTO Users (Id, Nombre, Apellido, Email, Deleted) 
                        VALUES (@Id, @Nombre, @Apellido, @Email, @Deleted);";

            return await _dbConnection.ExecuteAsync(sql, user) > 0;
        }


        public async Task<User?> GetAsync(string id)
        {
            var sql = "SELECT * FROM Users WHERE Id = @Id AND Deleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<bool> UpdateAsync(string id, DataUserDto user)
        {
            var userToUpdate = _mapper.Map<User>(user);
            userToUpdate.Id = id;
            var sql = "UPDATE Users SET Nombre = @Nombre, Apellido = @Apellido, Correo = @Correo WHERE Id = @Id AND Deleted = 0";
            return await _dbConnection.ExecuteAsync(sql, userToUpdate) > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var sql = "UPDATE Users SET Deleted = 1 WHERE Id = @Id AND Deleted = 0";
            return await _dbConnection.ExecuteAsync(sql, new { Id = id }) > 0;
        }
    }
}
