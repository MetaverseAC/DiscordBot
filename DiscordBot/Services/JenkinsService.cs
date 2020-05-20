using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DiscordBot.Services
{
	public class JenkinsService
	{
		private HttpClient _http;
		private IServiceProvider _services;
		private string _token;

		public const string JENKINS_URL = @"https://jenkins.metaverse.ac:8222";

		public JenkinsService(IServiceProvider services)
		{
			_http = services.GetRequiredService<HttpClient>();
			_services = services;
		}

		public void Initiaize(string token)
		{
			_token = token;
		}

		public async Task RunJobAsync(string job, string cause = "Run through DiscordBot JenkinsService")
		{
			var uri = $"{JENKINS_URL}/buildByToken/build?job={HttpUtility.UrlEncode(job)}&token={_token}";
			
			if (!string.IsNullOrWhiteSpace(cause))
			{
				uri += $"&cause={HttpUtility.UrlEncode(cause)}";
			}

			var resp = await _http.GetAsync(uri);
			resp.EnsureSuccessStatusCode();
		}
	}
}
