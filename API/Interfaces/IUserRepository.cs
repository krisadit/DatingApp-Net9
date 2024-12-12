﻿using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser?> GetByIdAsync(int id);
        Task<AppUser?> GetByNameAsync(string username);
        Task<IEnumerable<MemberDTO>> GetAllMembersAsync();
        Task<MemberDTO?> GetMemberAsync(string username);
    }
}