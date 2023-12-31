using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CSBackend.Models;

public class User : Model
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }
	[BsonElement("name")]
	public required string Name { get; set; }
	[BsonElement("username")]
	public required string Username { get; set; }
	[BsonElement("password")]
	public required string Password { get; set; }
	[BsonElement("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	[BsonElement("updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	public PublicInfo GetPublicInfo()
	{
		return new PublicInfo { Id = Id, Name = Name, Username = Username };
	}
}

public class PublicInfo
{
	public string? Id { get; set; }
	public required string Name { get; set; }
	public required string Username { get; set; }
}