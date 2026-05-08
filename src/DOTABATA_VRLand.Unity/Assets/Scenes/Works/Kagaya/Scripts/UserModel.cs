using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.Services;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using MagicOnion.Client;
using System;
using UnityEngine;

public class UserModel : Singleton<UserModel> {
    protected const string ServerURL = "http://10.70.41.152:5244";

    private GrpcChannelx channelx;
    private IUserService client;

    /// <summary>
    /// MagicOnion接続処理
    /// </summary>
    public UniTask CreateUserModel() {
        channelx = GrpcChannelx.ForAddress(ServerURL);
        client = MagicOnionClient.Create<IUserService>(channelx);

        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 全ユーザー情報取得
    /// </summary>
    public async UniTask<User[]> GetAllUsersAsync() {
        try {
            return await client.GetAllUsersAsync();
        }catch(Exception e) {
            Debug.LogException(e);
            throw;
        }
    }

    /// <summary>
    /// Idからユーザー情報取得
    /// </summary>
    public async UniTask<User> GetUserFromIdAsync(int id) {
        try {
            return await client.GetUserFromIdAsync(id);
        }catch (Exception e) {
            Debug.LogException(e);
            throw;
        }
    }

    /// <summary>
    /// ユーザー登録
    /// </summary>
    public async UniTask<bool> RegistUserAsync(string name) {
        try {
            return await client.RegistUserAsync(name);
        }catch(Exception e) {
            Debug.LogException(e);
            throw;
        }
    }
}
