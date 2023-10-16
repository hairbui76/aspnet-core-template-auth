using CSBackend.Configs;
using CSBackend.Models;
using MongoDB.Driver;

namespace CSBackend.Services;

public class UserService
{
	private readonly IMongoCollection<User> _usersCollection;
	private readonly string collectionName = "users";

	public UserService(IMongoClient mongoClient)
	{
		var mongoDatabase = mongoClient.GetDatabase(Config.DB.DATABASE);
		_usersCollection = mongoDatabase.GetCollection<User>(collectionName);
	}

	public async Task<List<User>> GetAsync() =>
			await _usersCollection.Find(_ => true).ToListAsync();

	public async Task<User?> GetAsync(string id) =>
			await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

	public async Task CreateAsync(User newUser) =>
			await _usersCollection.InsertOneAsync(newUser);

	public async Task UpdateAsync(string id, User updatedUser) =>
			await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

	public async Task RemoveAsync(string id) =>
			await _usersCollection.DeleteOneAsync(x => x.Id == id);
}