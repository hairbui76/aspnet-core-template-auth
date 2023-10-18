using CSBackend.Configs;
using CSBackend.Models;
using Microsoft.AspNetCore.DataProtection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace CSBackend.Services;

public class UserService
{
	private readonly IMongoCollection<User> _usersCollection;
	private readonly IDatabase _redis;
	private readonly IDataProtectionProvider _dataProtectionProvider;
	private readonly IDataProtector _protector;
	private readonly string collectionName = "users";

	public UserService(IMongoClient mongoClient, IConnectionMultiplexer muxer, IDataProtectionProvider dataProtectionProvider)
	{
		var mongoDatabase = mongoClient.GetDatabase(Config.DB.DATABASE);
		_usersCollection = mongoDatabase.GetCollection<User>(collectionName);
		_dataProtectionProvider = dataProtectionProvider;
		_protector = _dataProtectionProvider.CreateProtector("auth");
		_redis = muxer.GetDatabase();
	}

	public async Task<List<User>> GetAsync() =>
			await _usersCollection.Find(_ => true).ToListAsync();

	public async Task<User?> GetAsyncById(string id) =>
			await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

	public async Task<User?> GetAsyncByUsername(string username) =>
			await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

	public async Task CreateAsync(User newUser) =>
			await _usersCollection.InsertOneAsync(newUser);

	public async Task UpdateAsync(string id, User updatedUser) =>
			await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

	public async Task RemoveAsync(string id) =>
			await _usersCollection.DeleteOneAsync(x => x.Id == id);

	public async Task<(string, DateTime)> PrepareAccessToken(PublicInfo info)
	{
		var accessToken = Configs.Paseto.Encode(info, Config.TOKEN.SECRET);
		var accessExp = DateTime.Now.AddHours(Config.TOKEN.TOKEN_EXPIRE_HOURS);
		await _redis.StringSetAsync("ac_" + info.Id, true, accessExp.TimeOfDay);
		accessToken = _protector.Protect(accessToken);
		return (accessToken, accessExp);
	}

	public async Task<(string, DateTime)> PrepareRefreshToken(PublicInfo info)
	{
		var refreshToken = Configs.Paseto.Encode(info, Config.TOKEN.REFRESH_SECRET);
		var refreshExp = DateTime.Now.AddDays(Config.TOKEN.REFRESH_TOKEN_EXPIRE_WEEKS * 7);
		await _redis.StringSetAsync("rf_" + info.Id, true, refreshExp.TimeOfDay);
		refreshToken = _protector.Protect(refreshToken);
		return (refreshToken, refreshExp);
	}
}