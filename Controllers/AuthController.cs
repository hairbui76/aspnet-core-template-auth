using CSBackend.Configs;
using CSBackend.Models;
using CSBackend.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CSBackend.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
	private readonly UserService _userService;
	private readonly ILogger<AuthController> _logger;
	private readonly IDataProtectionProvider _dataProtectionProvider;
	private readonly IDataProtector _protector;
	private readonly IDatabase _redis;

	public AuthController(ILogger<AuthController> logger, IDataProtectionProvider dataProtectionProvider, IConnectionMultiplexer muxer, UserService userService)
	{
		_logger = logger;
		_userService = userService;
		_dataProtectionProvider = dataProtectionProvider;
		_protector = _dataProtectionProvider.CreateProtector("auth");
		_redis = muxer.GetDatabase();
	}

	[HttpGet]
	[Route("/auth")]
	public async Task<IActionResult> Index()
	{
		User? user = (User?)HttpContext.Items["user"];
		if (user == null)
			return Unauthorized();
		bool? resetAccess = (bool?)HttpContext.Items["reset_access"];
		if (resetAccess == null || resetAccess == false)
			return Unauthorized();
		else
		{
			var info = user.GetPublicInfo();
			var accessToken = Configs.Paseto.Encode(info, Config.TOKEN.SECRET);
			var accessExp = DateTime.Now.AddHours(Config.TOKEN.TOKEN_EXPIRE_HOURS);
			await _redis.StringSetAsync("ac_" + user.Id, true, accessExp.TimeOfDay);
			accessToken = _protector.Protect(accessToken);
			Response.Cookies.Append("access_token", accessToken, new CookieOptions
			{
				Secure = Config.ENV == "production",
				HttpOnly = true,
				Path = "/",
				Expires = accessExp,
			});
			return Ok(new { message = "Authenticated!!", user = info });
		}
	}

	[HttpGet]
	public async Task<List<User>> Get() => await _userService.GetAsync();

	[HttpGet("{id:length(24)}")]
	public async Task<ActionResult<User>> Get(string id)
	{
		var user = await _userService.GetAsync(id);
		if (user == null)
			return NotFound();
		return user;
	}

	[HttpPost]
	public string Login(string username, string password)
	{
		Console.WriteLine(username, password);
		return "login" + username + " " + password;
	}

	[HttpPost]
	public async Task<IActionResult> Register(User newUser)
	{
		newUser.Password = Password.Hash(newUser.Password);
		await _userService.CreateAsync(newUser);
		var info = newUser.GetPublicInfo();
		var accessToken = Configs.Paseto.Encode(info, Config.TOKEN.SECRET);
		var refreshToken = Configs.Paseto.Encode(info, Config.TOKEN.REFRESH_SECRET);
		var accessExp = DateTime.Now.AddHours(Config.TOKEN.TOKEN_EXPIRE_HOURS);
		var refreshExp = DateTime.Now.AddDays(Config.TOKEN.REFRESH_TOKEN_EXPIRE_WEEKS * 7);
		var a = _redis.StringSetAsync("ac_" + newUser.Id, true, accessExp.TimeOfDay);
		var b = _redis.StringSetAsync("rf_" + newUser.Id, true, refreshExp.TimeOfDay);
		await Task.WhenAll(a, b);
		accessToken = _protector.Protect(accessToken);
		refreshToken = _protector.Protect(refreshToken);
		Response.Cookies.Append("access_token", accessToken, new CookieOptions
		{
			Secure = Config.ENV == "production",
			HttpOnly = true,
			Path = "/",
			Expires = accessExp,
		});
		Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
		{
			Secure = Config.ENV == "production",
			HttpOnly = true,
			Path = "/",
			Expires = refreshExp,
		});
		return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
	}
}