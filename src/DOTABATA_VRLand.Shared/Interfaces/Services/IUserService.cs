using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;

namespace DOTABATA_VRLand.Shared.Interfaces.Services {
    public interface IUserService : IService<IUserService> {
        /// <summary>
        /// 全ユーザー情報取得
        /// </summary>
        UnaryResult<User[]> GetAllUsersAsync();

        /// <summary>
        /// ユーザー登録
        /// </summary>
        UnaryResult<bool> RegistUserAsync(string name);
    }
}
