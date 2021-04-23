using Discord;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot.Services
{
	public class FormatCodeService : BrackeysBotService
	{
		private readonly DataService _data;
		
		/// <inheritdoc />
		public FormatCodeService(DataService data)
		{
			_data = data;
		}

		public int GetTimeoutSetting() => _data.Configuration.CodeFormatterDeleteTresholdMillis;

		// Formats the input using microsoft's code parser
		public string Format(string input)
		{
			var tree = CSharpSyntaxTree.ParseText(input);
			var node = tree.GetRoot().NormalizeWhitespace();
			return node.ToFullString();
		}
	}
}
