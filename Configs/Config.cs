using DotNetEnv;

namespace CSBackend.Configs;

public class Config
{
	public static Base BASE = new();
	public static Db DB = new();
	public static Token TOKEN = new();
	public static string? ENV = Environment.GetEnvironmentVariable("ENV");
}

public class Base
{
	public string PORT;
	public string HOSTNAME;
	public Base()
	{
		PORT = Environment.GetEnvironmentVariable("BASE_PORT") ?? throw new NullReferenceException("BASE_PORT is null");
		HOSTNAME = Environment.GetEnvironmentVariable("BASE_HOST") ?? throw new NullReferenceException("BASE_HOST is null");
	}
}

public class Db
{
	public string URL;
	public string HOST;
	public string PORT;
	public string DATABASE;
	public Db()
	{
		HOST = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? throw new NullReferenceException("MONGODB_HOST is null");
		PORT = Environment.GetEnvironmentVariable("MONGODB_PORT") ?? throw new NullReferenceException("MONGODB_PORT is null");
		URL = Environment.GetEnvironmentVariable("MONGODB_URL") ?? GetConnectionString();
		DATABASE = Environment.GetEnvironmentVariable("MONGODB_DB_NAME") ?? throw new NullReferenceException("MONGODB_DB_NAME is null");
	}
	public string GetConnectionString()
	{
		if (URL != null)
			return URL;
		return string.Format("mongodb://{0}:{1}", HOST, PORT);
	}
}

public class Token
{
	public string SECRET;
	public double TOKEN_EXPIRE_HOURS;
	public string REFRESH_SECRET;
	public int REFRESH_TOKEN_EXPIRE_WEEKS;
	public Token()
	{
		SECRET = Environment.GetEnvironmentVariable("TOKEN_SECRET") ?? throw new NullReferenceException("TOKEN_SECRET is null");
		string hours = Environment.GetEnvironmentVariable("TOKEN_EXPIRE_HOURS") ?? throw new NullReferenceException("TOKEN_EXPIRE_HOURS is null");
		TOKEN_EXPIRE_HOURS = double.Parse(hours);
		REFRESH_SECRET = Environment.GetEnvironmentVariable("TOKEN_REFRESH_SECRET") ?? throw new NullReferenceException("TOKEN_REFRESH_SECRET is null");
		string weeks = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRE_WEEKS") ?? throw new NullReferenceException("REFRESH_TOKEN_EXPIRE_WEEKS is null");
		REFRESH_TOKEN_EXPIRE_WEEKS = int.Parse(weeks);
	}
}