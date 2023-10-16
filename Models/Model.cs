using Newtonsoft.Json;

namespace CSBackend.Models;

public class Model
{
	public string Repr() => JsonConvert.SerializeObject(this);
}