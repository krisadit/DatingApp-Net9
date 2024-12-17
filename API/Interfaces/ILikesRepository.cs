﻿using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId);
        Task<IEnumerable<MemberDTO>> GetUserLikes(string predicates, int userId);
        Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
        void DeleteLike(UserLike like);
        void AddLike(UserLike like);
        Task<bool> SaveChanges();
    }
}