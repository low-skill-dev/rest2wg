﻿namespace vdb_node_api.Models.Runtime
{
	public class JwtServiceSettings
	{
		public virtual double TokenLifespanDays { get; set; } = 365;

		public virtual double AccessLifespanDays { get; set; } = 0.1;
		public virtual double RefreshLifespanDays { get; set; } = 30;

		// this fields must be left null so it will throw if
		// the signing key or issuer are not set by secrets.json.
		// that prevents going to release with a default key
		public virtual string SigningKey { get; set; } = null!;
		public virtual string Issuer { get; set; } = null!;
	}
}
